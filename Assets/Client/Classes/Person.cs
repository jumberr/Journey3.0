using UnityEngine;

namespace Client.Classes
{
    public abstract class Person
    {
        private float health;
        private float maxHealth;
        private float moveSpeed;
        public bool IsDead => Health.Equals(0);
        public delegate void OnHealthZero();
        public event OnHealthZero healthZero;
        protected Person(float health, float maxHealth, float moveSpeed)
        {
            this.health = health;
            this.maxHealth = maxHealth;
            this.moveSpeed = moveSpeed;
            healthZero += Die;
        }

        public float Health
        {
            get => health;
            private set
            {
                health = Mathf.Clamp(value, 0, MaxHealth);
                if(health==0)
                    healthZero?.Invoke();
            }
        }

        public float MaxHealth
        {
            get => maxHealth;
            private set => maxHealth = value;
        }

        public float MoveSpeed
        {
            get => moveSpeed;
            private set => moveSpeed = value;
        }

        public virtual void Die() {}

        public virtual void TakeDamage(float damage)
        {
            Health -= damage;
        }

        public virtual void RestoreHealth(float heal)
        {
            Health += heal;
        }
    }
}