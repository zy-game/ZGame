namespace ZEngine.World
{
    public class PhysicsComponent : EntityComponent
    {
        internal void OnEntry(PhysicsComponent component)
        {
            OnTriggerEntry(component);
        }

        internal void OnExit(PhysicsComponent component)
        {
            OnTriggerExit(component);
        }

        internal void OnStay(PhysicsComponent component)
        {
            OnTriggerStay(component);
        }

        protected virtual void OnTriggerEntry(PhysicsComponent component)
        {
        }

        protected virtual void OnTriggerExit(PhysicsComponent component)
        {
        }

        protected virtual void OnTriggerStay(PhysicsComponent component)
        {
        }
    }
}