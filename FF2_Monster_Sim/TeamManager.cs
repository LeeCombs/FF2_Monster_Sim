using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FF2_Monster_Sim
{
    public static class TeamManager
    {
        public struct Team
        {
            public string TeamName, TeamString;
            public int TeamIndex, Wins, Losses, Ties, Rounds;
        }

        private static List<string> teamData;
        private const string TEAM_DATA_PATH = @".\Content\Data\FF2_TeamData.csv";
        
        //////////////
        // Monogame //
        //////////////

        public static void Initialize()
        {
            teamData = File.ReadAllLines(TEAM_DATA_PATH).ToList();
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

        /// <summary>
        /// Retrieve a random team string
        /// </summary>
        /// <returns></returns>
        public static string GetRandomTeamString()
        {
            // Ignore the 0 index random team
            return teamData[Globals.rnd.Next(1, teamData.Count)];
        }

        /// <summary>
        /// Retrieve a random team
        /// </summary>
        /// <returns></returns>
        public static Team GetRandomTeam()
        {
            return TeamFromString(GetRandomTeamString());
        }

        /// <summary>
        /// Retrieve a team by its index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Team GetTeamByIndex(int index)
        {
            // TODO: Error checking
            return TeamFromString(teamData[index]);
        }
        
        /// <summary>
        /// Create a team from a given string.
        /// Example input: "12,It's All Ogre,B;WzOgre-WzOgre-GrOgre-GrOgre-Ogre-Ogre,1,4,0,28"
        /// </summary>
        public static Team TeamFromString(string str)
        {
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

        /// <summary>
        /// Convert a Team to its string representation
        /// Example output: "12,It's All Ogre,B;WzOgre-WzOgre-GrOgre-GrOgre-Ogre-Ogre,1,4,0,28"
        /// </summary>
        /// <param name="team">Team to convert</param>
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

        /// <summary>
        /// Get a string of the wins, losses, ties, and rounds played of a given team
        /// </summary>
        /// <returns>Wins, losses, ties, and rounds played of a given team</returns>
        public static string GetTeamInfo(Team team)
        {
            return "Wins: " + team.Wins.ToString() +
                "\nLoss: " + team.Losses.ToString() +
                "\nTies: " + team.Ties.ToString() +
                "\nRounds Played: " + team.Rounds.ToString();
        }

        /// <summary>
        /// Update a team's stat
        /// </summary>
        /// <param name="team"></param>
        /// <param name="rounds">The number of rounds elapsed during the match</param>
        /// <param name="winState">The state of 'winning' the team did (TODO: this sucks)</param>
        public static void UpdateTeamData(Team team, int rounds, int winState)
        {
            // winState, 0 = loss, 1 = win, 2 = tie
            if (winState == 0)
                team.Losses++;
            else if (winState == 1)
                team.Wins++;
            else if (winState == 2)
                team.Ties++;

            team.Rounds += rounds;

            teamData[team.TeamIndex] = TeamToString(team);
        }

        /// <summary>
        /// Write out all teams to the team data file
        /// </summary>
        public static void WriteTeamData()
        {
            try
            {
                File.WriteAllLines(TEAM_DATA_PATH, teamData);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error writing team data: " + e);
            }
        }

        /////////////
        // Helpers //
        /////////////
    }
}
