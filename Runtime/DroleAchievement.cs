using System;
namespace JTuresson.Social
{
    [Serializable]
    public struct DroleAchievement : IEquatable<DroleAchievement>
    {
        public double steps;
        public double stepsToComplete;
        public string id;
        public bool hasIncrement;
        public DroleAchievement(string id)
        {
            this.id = id;
            steps = 100;
            stepsToComplete = 100;
            hasIncrement = false;
        }
        public DroleAchievement(string id, double steps, double stepsToComplete)
        {
            this.id = id;
            this.steps = steps;
            this.stepsToComplete = stepsToComplete;
            hasIncrement = true;
        }

        public override bool Equals(object obj) => obj is DroleAchievement other && this.Equals(other);

        public bool Equals(DroleAchievement p) => id == p.id;

        public override int GetHashCode() => (id).GetHashCode();

        public static bool operator ==(DroleAchievement lhs, DroleAchievement rhs) => lhs.Equals(rhs);

        public static bool operator !=(DroleAchievement lhs, DroleAchievement rhs) => !(lhs == rhs);
    }
}
