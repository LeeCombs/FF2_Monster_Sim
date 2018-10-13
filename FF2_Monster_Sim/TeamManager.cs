using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF2_Monster_Sim
{
    public static class TeamManager
    {
        public struct Team
        {
            public string TeamName, TeamString;
            public int TeamIndex, Wins, Losses, Ties, Rounds;
        }


        //////////////
        // Monogame //
        //////////////

        public static void Initialize()
        {
            //
        }

        public static void LoadContent()
        {
            //
        }

        public static void Draw()
        {
            //
        }

        /////////////
        // Publics //
        /////////////

        public static Team TeamFromString(string str)
        {
            // Example input: "12,It's All Ogre,B;WzOgre-WzOgre-GrOgre-GrOgre-Ogre-Ogre,1,4,0,28"
            // System.Diagnostics.Debug.WriteLine("Creating team from: " + str);
            string[] strSplit = str.Split(',');
            return new Team
            {
                TeamIndex = int.Parse(strSplit[0]),
                TeamName = strSplit[1],
                TeamString = strSplit[2],
                Wins = int.Parse(strSplit[3]),
                Losses = int.Parse(strSplit[4]),
                Ties = int.Parse(strSplit[5]),
                Rounds = int.Parse(strSplit[6])
            };
        }

        public static string TeamToString(Team team)
        {
            return team.TeamIndex.ToString() +
                "," + team.TeamName +
                "," + team.TeamString +
                "," + team.Wins.ToString() +
                "," + team.Losses.ToString() +
                "," + team.Ties.ToString() +
                "," + team.Rounds.ToString();
        }

        public static string GetTeamInfo(Team team)
        {
            return "Wins: " + team.Wins.ToString() +
                "\nLoss: " + team.Losses.ToString() +
                "\nTies: " + team.Ties.ToString() +
                "\nRounds Played: " + team.Rounds.ToString();
        }

        /////////////
        // Helpers //
        /////////////

    }
}
