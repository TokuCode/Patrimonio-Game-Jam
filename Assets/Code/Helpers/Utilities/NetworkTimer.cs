namespace Movement3D.Helpers
{
    public class Metronome
    {
        float timer;
        public float MinTimeBetweenTicks { get; private set; }
        
        public Metronome(float tickRate) 
        {
            MinTimeBetweenTicks = 1f / tickRate;
        }

        public void Update(float deltaTime)
        {
            timer += deltaTime;
        }
        
        public bool ShouldTick() 
        {
            if (timer >= MinTimeBetweenTicks) 
            {
                timer -= MinTimeBetweenTicks;
                return true;
            }
            return false;
        }

        public void Reset(float newTickRate = 0)
        {
            if(newTickRate != 0) MinTimeBetweenTicks = 1 / newTickRate;
            timer = 0;
        }
    }
}