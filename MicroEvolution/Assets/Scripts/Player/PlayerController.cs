using MicroEvolution.Core;
using MicroEvolution.Evolution;
using MicroEvolution.InputSystem;
using MicroEvolution.Visuals;
using UnityEngine;

namespace MicroEvolution.Player
{
    [RequireComponent(typeof(CellMotor))]
    [RequireComponent(typeof(LivingCell))]
    public class PlayerController : MonoBehaviour
    {
        CellMotor _motor;
        LivingCell _cell;
        CellAppearance _appearance;
        bool _boosting;
        float _chemCooldown;
        float _toxinTick;
        Vector3 _spawnPos;

        public float ChemCooldownRemaining => Mathf.Max(0f, _chemCooldown);
        public bool Boosting => _boosting;
        public float ChemCooldownNormalized =>
            Mathf.Clamp01(_chemCooldown / GameConfig.ChemosynthesisCooldown);

        public void Init(Vector3 spawnPos)
        {
            _spawnPos = spawnPos;
            _motor = GetComponent<CellMotor>();
            _cell = GetComponent<LivingCell>();
            _appearance = GetComponent<CellAppearance>();

            _cell.Configure(
                Faction.Player,
                GameConfig.PlayerRadius,
                GameConfig.PlayerMaxHealth,
                16f,
                0f,
                0,
                true);

            _motor.MaxSpeed = GameConfig.PlayerBaseSpeed;
            if (GameState.Instance != null)
                GameState.Instance.PlayerTransform = transform;
        }

        void Update()
        {
            if (GameState.Instance == null) return;
            if (GameFlow.Instance != null && GameFlow.Instance.Screen != AppScreen.Playing)
            {
                _motor.SetDesiredVelocity(Vector2.zero);
                return;
            }

            if (GameState.Instance.PlayerDead)
            {
                _motor.SetDesiredVelocity(Vector2.zero);
                if (UnityEngine.Input.GetKeyDown(KeyCode.R) ||
                    (GameInput.Instance != null && GameInput.Instance.ChemPressed))
                {
                    transform.position = _spawnPos;
                    GameState.Instance.RespawnPlayer();
                }

                return;
            }

            if (_chemCooldown > 0f) _chemCooldown -= Time.deltaTime;
            SyncEvolutionVisuals();

            var input = GameInput.Instance != null ? GameInput.Instance.Move : Vector2.zero;
            _boosting = (GameInput.Instance != null && GameInput.Instance.BoostHeld) &&
                        input.sqrMagnitude > 0.01f && GameState.Instance.Atp > 1f;

            var speed = GameConfig.PlayerBaseSpeed;
            if (GameState.Instance.HasOscillator) speed *= GameConfig.OscillatorSpeedBonus;
            if (GameState.Instance.HasFlagella) speed *= GameConfig.FlagellaSpeedBonus;
            if (_boosting) speed *= GameConfig.PlayerBoostMultiplier;

            _motor.MaxSpeed = speed;
            _motor.Acceleration = GameState.Instance.HasFlagella ? 24f : 18f;
            _motor.SetDesiredVelocity(input.normalized * speed);
            _appearance?.SetBoostVisual(_boosting);

            var atpDelta = GameConfig.AtpRegenPerSecond * Time.deltaTime;
            if (input.sqrMagnitude > 0.01f)
                atpDelta -= GameConfig.MoveAtpCostPerSecond * Time.deltaTime;
            if (_boosting)
                atpDelta -= GameConfig.BoostAtpCostPerSecond * Time.deltaTime;
            GameState.Instance.AddAtp(atpDelta);

            if (GameInput.Instance != null && GameInput.Instance.ChemPressed)
                TryChemosynthesis();

            HandleHotkeys();
            TickToxin();
        }

        void HandleHotkeys()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
                EvolutionShop.TryBuyOscillator();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2))
                EvolutionShop.TryBuySpikes();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad3))
                EvolutionShop.TryBuyMembrane();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad4))
                EvolutionShop.TryBuyChemosynthesis();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad5))
                EvolutionShop.TryBuyFlagella();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha6) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad6))
                EvolutionShop.TryBuyEyes();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha7) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad7))
                EvolutionShop.TryBuyJaws();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha8) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad8))
                EvolutionShop.TryBuyToxin();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha9) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad9))
                EvolutionShop.TryBuyStorage();
        }

        void TickToxin()
        {
            if (!GameState.Instance.HasToxin) return;
            _toxinTick -= Time.deltaTime;
            if (_toxinTick > 0f) return;
            _toxinTick = 0.5f;

            foreach (var cell in CellRegistry.All)
            {
                if (cell == null || cell.IsPlayer) continue;
                if (cell.Faction != Faction.Prey && cell.Faction != Faction.Predator) continue;
                if ((cell.transform.position - transform.position).sqrMagnitude > 4.5f * 4.5f) continue;
                cell.ApplyDamage(GameConfig.ToxinDps * 0.5f, _cell);
            }
        }

        void SyncEvolutionVisuals()
        {
            _appearance?.SetSpikesVisible(GameState.Instance.HasSpikes);
            _appearance?.SetFlagellaVisible(GameState.Instance.HasFlagella || GameState.Instance.HasOscillator);
            _appearance?.SetMembraneColor(GameState.Instance.MembraneColor);
        }

        void TryChemosynthesis()
        {
            if (_chemCooldown > 0f) return;

            var restore = GameConfig.ChemosynthesisAtpRestore;
            var heal = GameConfig.ChemosynthesisHeal;
            if (GameState.Instance.HasChemosynthesisUpgrade)
            {
                restore *= 1.5f;
                heal *= 1.4f;
            }

            GameState.Instance.AddAtp(restore);
            GameState.Instance.Heal(heal);
            _chemCooldown = GameConfig.ChemosynthesisCooldown;
            VfxBurst.Spawn(transform.position, new Color(0.3f, 0.8f, 1f, 0.8f), 1.4f);
            GameEvents.RaiseToast("Chemosynthesis — metabolic burst");
        }
    }
}
