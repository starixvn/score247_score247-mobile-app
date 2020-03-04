using LiveScore.Core.Enumerations;

namespace LiveScore.Soccer.Enumerations
{
    /// <summary>
    /// Keep using image for dynamic loading
    /// </summary>
    public class Images : TextEnumeration
    {
        public static readonly Images YellowCard = new Images("images/common/yellow_card.svg", nameof(YellowCard));
        public static readonly Images RedYellowCard = new Images("images/common/red_yellow_card.svg", nameof(RedYellowCard));
        public static readonly Images RedCard = new Images("images/common/red_card.svg", nameof(RedCard));
        public static readonly Images OwnGoal = new Images("images/common/own_goal.svg", nameof(OwnGoal));
        public static readonly Images Goal = new Images("images/common/ball.svg", nameof(Goal));
        public static readonly Images PenaltyShootoutGoal = new Images("images/common/ball.svg", nameof(PenaltyShootoutGoal));
        public static readonly Images MissedPenaltyShootoutGoal = new Images("images/common/missed_goal.svg", nameof(MissedPenaltyShootoutGoal));
        public static readonly Images GoalAssist = new Images("images/common/assist_goal.svg", nameof(GoalAssist));
        public static readonly Images PenaltyGoal = new Images("images/common/penalty_goal.svg", nameof(PenaltyGoal));
        public static readonly Images MissPenaltyGoal = new Images("images/common/missed_penalty_goal.svg", nameof(PenaltyGoal));
        public static readonly Images Substitution = new Images("images/common/substitution.svg", nameof(Substitution));
        public static readonly Images SubstitutionIn = new Images("images/common/substitution_in.svg", nameof(SubstitutionIn));
        public static readonly Images SubstitutionOut = new Images("images/common/substitution_out.svg", nameof(SubstitutionOut));

        public static readonly Images SecondLeg = new Images("images/common/second_leg_winner.png", nameof(SecondLeg));
        public static readonly Images PenaltyWinner = new Images("images/common/penalty_winner.png", nameof(PenaltyWinner));
        public static readonly Images ScoreChangeByOwnGoal = new Images("images/common/own_goal_substitution.svg", nameof(ScoreChangeByOwnGoal));
        public static readonly Images Var = new Images("images/common/var.svg", nameof(Var));

        public Images()
        {
        }

        protected Images(string value, string name) : base(value, name)
        {
        }
    }
}