using MicroEvolution.Core;
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
        Vector3 _spawnPos;

        public float ChemCooldownRemaining => Mathf.Max(0f, _chemCooldown);
        public bool Boosting => _boosting;

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

            if (GameState.Instance.PlayerDead)
            {
                _motor.SetDesiredVelocity(Vector2.zero);
                if (Input.GetKeyDown(KeyCode.R))
                {
                    transform.position = _spawnPos;
                    GameState.Instance.RespawnPlayer();
                }

                return;
            }

            if (_chemCooldown > 0f) _chemCooldown -= Time.deltaTime;

            SyncEvolutionVisuals();

            var input = ReadMoveInput();
            _boosting = Input.GetKey(KeyCode.Q) && input.sqrMagnitude > 0.01f && GameState.Instance.Atp > 1f;

            var speed = GameConfig.PlayerBaseSpeed;
            if (GameState.Instance.HasOscillator) speed *= GameConfig.OscillatorSpeedBonus;
            if (_boosting) speed *= GameConfig.PlayerBoostMultiplier;

            _motor.MaxSpeed = speed;
            _motor.SetDesiredVelocity(input.normalized * speed);
            _appearance?.SetBoostVisual(_boosting);

            // ATP economy
            var atpDelta = GameConfig.AtpRegenPerSecond * Time.deltaTime;
            if (input.sqrMagnitude > 0.01f)
                atpDelta -= GameConfig.MoveAtpCostPerSecond * Time.deltaTime;
            if (_boosting)
                atpDelta -= GameConfig.BoostAtpCostPerSecond * Time.deltaTime;
            GameState.Instance.AddAtp(atpDelta);

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
                TryChemosynthesis();

            // Hotkeys 1-4 for evolution purchases
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                Evolution.EvolutionShop.TryBuyOscillator();
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                Evolution.EvolutionShop.TryBuySpikes();
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                Evolution.EvolutionShop.TryBuyMembrane();
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                Evolution.EvolutionShop.TryBuyChemosynthesis();
        }

        void SyncEvolutionVisuals()
        {
            _appearance?.SetSpikesVisible(GameState.Instance.HasSpikes);
        }

        void TryChemosynthesis()
        {
            if (_chemCooldown > 0f) return;

            // Ability is always available; upgrade improves payoff.
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
            GameEvents.RaiseToast("Chemosynthesis — metabolic burst");
        }

        static Vector2 ReadMoveInput()
        {
            var x = 0f;
            var y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;

            if (Input.GetMouseButton(1) && Camera.main != null && GameState.Instance.PlayerTransform != null)
            {
                var world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                world.z = 0f;
                var to = (Vector2)(world - GameState.Instance.PlayerTransform.position);
                if (to.sqrMagnitude > 0.01f)
                    return to.normalized;
            }

            return new Vector2(x, y);
        }
    }
}
