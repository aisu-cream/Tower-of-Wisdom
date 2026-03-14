using UnityEngine;

namespace ImprovedTimers {
    public class CountdownTimer : Timer {
        public CountdownTimer(float value) : base(value) { }

        public override void Tick() {
            if (!IsRunning) return;
            if (CurrentTime > 0) CurrentTime -= Time.deltaTime;
            if (CurrentTime <= 0) Stop();
        }

        public override bool IsFinished => CurrentTime <= 0;
    }
}