using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{
    public enum SceneType
    {
        A, B, C
    }

    public struct Action
    {
        public Monster Actor;
        public List<Monster> Targets;
        public bool Physical;
        public bool Nothing;
        public Spell Spell;
        public int SpellLevel;

        public Action(Monster actor)
        {
            Actor = actor;
            Targets = new List<Monster>();
            Physical = false;
            Nothing = false;
            Spell = null;
            SpellLevel = 0;
        }
    }

    public class BattleScene
    {
        public SceneType SceneType { get; private set; }
        public bool Flipped = false, IsTeam = false;
        private int sceneNum;
        public int X = 0, Y = 0;
        
        private Dictionary<int, Vector2[]> slotPositions = new Dictionary<int, Vector2[]>();
        
        private MonoMonster[][] columns = new MonoMonster[4][];
        
        // TEMP
        public string MonsterNames;
        public string SceneString;

        public BattleScene(int sceneNum, int x, int y, bool flipped = false)
        {
            this.sceneNum = sceneNum;
            X = x;
            Y = y;
            Flipped = flipped;
            Initialize();
        }

        //////////////
        // Monogame //
        //////////////

        public void Initialize()
        {
            // Populate columns with nulls to start
            for (int i = 0; i < columns.Length; i++)
                columns[i] = new MonoMonster[] { null, null };
        }

        public void LoadContent()
        {
            //
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (MonoMonster[] col in columns)
                foreach (MonoMonster mon in col)
                    if (mon != null)
                        if (mon.IsAlive())
                            mon.Draw(spriteBatch);
        }
        
        /////////////
        // Publics //
        /////////////

        /// <summary>
        /// Setup and populate the scene with monsters using a very specifically formatted string...
        /// Expected format: "A;name-name-name-name-name-name-name-name"
        /// </summary>
        public void PopulateScene(string sceneString, ContentManager content, bool isTeam = false)
        {
            // Cleanup any leftovers first
            ClearScene();

            IsTeam = isTeam;

            // Rip apart the string into it's scene type and monster list
            SceneString = sceneString;
            string[] sceneSplit = sceneString.Split(';');
            string sType = sceneSplit[0];
            string monsterNames = sceneSplit[1];
            
            // Setup the scene, then populate with the monsters
            SceneType = (SceneType)Enum.Parse(typeof(SceneType), sType);
            LoadSceneType(SceneType);

            int col = 0, row = 0;
            foreach (string name in monsterNames.Split('-'))
            {
                MonoMonster monster = MonoMonsterManager.GetMonoMonsterByName(name);
                if (monster == null)
                    continue;
                
                if (content != null)
                    monster.Initialize(content.Load<Texture2D>("Graphics\\Monsters\\" + monster.Name), Flipped);

                monster.scene = this;

                columns[col / 2][row % 2] = monster;
                monster.Position = slotPositions[col / 2][row % 2];
                row++;
                col++;

                // Tall monsters skip the next slot
                if (string.Equals(monster.size.ToUpper(), "TALL"))
                {
                    row++;
                    col++;
                }
            }

            // Set the display text for monsters
            UpdateSceneText();
        }
        
        /// <summary>
        /// Clear text display and remove all monsters from the scene
        /// </summary>
        public void ClearScene()
        {
            TextManager.SetSceneText(sceneNum, "");
            
            for (int i = 0; i < columns.Length; i++)
                for (int j = 0; j < columns[i].Length; j++)
                    if (columns[i][j] != null)
                    {
                        columns[i][j].Kill();
                        columns[i][j] = null;
                    }
        }

        /// <summary>
        /// Generate and return a list of actions from active monsters
        /// </summary>
        /// <param name="sceneRef">The target scene to target monsters from</param>
        public List<Action> GetMonsterActions(BattleScene sceneRef)
        {
            // Get the monster's action at a given position
            List<Action> actList = new List<Action>();

            foreach (Monster mon in GetAllLiveMonsters())
            {
                if (mon == null)
                    continue;

                // Rolls the monster's initiative and figure out it's action
                mon.Init = mon.Evasion += Globals.rnd.Next(0, 50);
                Action action = new Action(mon);
                MonsterAction monAct = mon.GetAction();
                
                if (monAct.Name == "Attack")
                {
                    // If monster is in back row, it will instead return 'nothing'
                    if (MonsterIsBackRow(mon))
                    {
                        action.Nothing = true;
                        action.Targets.Add(mon);
                        actList.Add(action);
                        continue;
                    }

                    action.Physical = true;

                    // Confused monsters target their allies and themselves
                    if (mon.HasTempStatus(TempStatus.Confuse))
                        action.Targets.Add(this.GetFrontRowTarget());
                    else
                        action.Targets.Add(sceneRef.GetFrontRowTarget());

                    actList.Add(action);
                    continue;
                }
                else
                {
                    // Get Spell
                    Spell spell = SpellManager.GetSpellByName(monAct.Name);
                    spell.Accuracy = monAct.Accuracy;
                    action.Spell = spell;
                    action.SpellLevel = monAct.Level;

                    // Deal mp cost here, why not
                    mon.MP -= monAct.MPCost;

                    switch (monAct.Target)
                    {
                        case "Self":
                            action.Targets.Add(mon);
                            break;
                        case "SingleTarget":
                            action.Targets.Add(sceneRef.GetAnySingleTarget());
                            break;
                        case "EnemyParty":
                            action.Targets = sceneRef.GetAllLiveMonsters().ToList();
                            break;
                        case "CasterParty":
                            action.Targets = this.GetAllLiveMonsters().ToList();
                            break;
                        default:
                            throw new Exception("Invalid monAct target: " + monAct.Target);
                    }
                    actList.Add(action);
                }
            }

            return actList;
        }
        
        /// <summary>
        /// Retrieve an array of all live monsters
        /// </summary>
        public Monster[] GetAllLiveMonsters()
        {
            List<Monster> liveMonsters = new List<Monster>();
            foreach (Monster[] col in columns)
                foreach (Monster mon in col)
                    if (mon != null)
                        if (mon.IsAlive())
                            liveMonsters.Add(mon);
            return liveMonsters.ToArray();
        }

        /// <summary>
        /// Return whether the scene has living creatures
        /// </summary>
        public bool HasLivingMonsters()
        {
            return GetAllLiveMonsters().Length > 0;
        }

        /// <summary>
        /// Retrieve a single random Monster from any slot
        /// </summary>
        public Monster GetAnySingleTarget()
        {
            // Build a list of current monsters, choose one randomly and return it
            Monster[] activeList = GetAllLiveMonsters();
            int slotRoll = Globals.rnd.Next(0, activeList.Length);
            return activeList[slotRoll];
        }

        /// <summary>
        /// Retrieve a single random monster from the front rows
        /// </summary>
        public Monster GetFrontRowTarget()
        {
            // Build a list of viable targets based on which two rows are front rows
            List<Monster> monsterList = new List<Monster>();

            foreach (Monster monst in GetAllLiveMonsters())
                if (!MonsterIsBackRow(monst))
                    monsterList.Add(monst);
            
            // Roll a random monster from the list and return it
            int slotRoll = Globals.rnd.Next(0, monsterList.Count);
            return monsterList[slotRoll];
        }

        /// <summary>
        /// Iterate through all monsters and build the display text based on their current stats
        /// </summary>
        public void UpdateSceneText()
        {
            string displayText = "";
            foreach (Monster monster in GetAllMonsters())
            {
                // Set line color based on Monster's current HP. Red = dead, Yellow = half or less.
                if (monster.IsDead())
                    displayText += "{{Red}}";
                else if (monster.IsCritical())
                    displayText += "{{White}}";
                else
                    displayText += "{{Yellow}}";

                displayText += monster.Name.PadRight(9) + " - ";
                displayText += monster.HP.ToString().PadLeft(6) + " - ";
                displayText += monster.MP.ToString().PadLeft(6) + "\n";
            }
            TextManager.SetSceneText(sceneNum, displayText);
        }

        /////////////
        // Helpers //
        /////////////
        
        /// <summary>
        /// Sets the scene's type, monster slots, and positions, based on supplied sceneType
        /// </summary>
        private void LoadSceneType(SceneType sceneType)
        {
            // Cleanup before populating the scene
            ClearScene();

            // Set monster positions based on scene type, then flip them if necessary
            SceneType = sceneType;
            switch (SceneType)
            {
                case SceneType.A: // 4 x 2 Slots
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2[] positions = { new Vector2(X + 75 * i, Y + 0), new Vector2(X + 75 * i, Y + 120) }; // Y + 100 pre-status
                        slotPositions[i] = positions;
                    }
                    break;
                case SceneType.B: // 3 x 2 Slots
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2[] positions = { new Vector2(X + 100 * i, Y + 0), new Vector2(X + 100 * i, Y + 120) }; // Y + 100 pre-status
                        slotPositions[i] = positions;
                    }
                    break;
                case SceneType.C: // Only one slot
                    slotPositions[0] = new Vector2[] { new Vector2(X, Y) };
                    return;
                default:
                    throw new Exception("Invalid SceneType supplied: " + sceneType);
            }

            // Reverse positions if the scene is flipped
            if (Flipped)
                Utils.ReverseNumberedDictValues(ref slotPositions);
        }

        /// <summary>
        /// Retrieve an array of all alive and non-null monsters from a given column
        /// </summary>
        private Monster[] GetLiveMonstersFromCol(int col)
        {
            List<Monster> monList = new List<Monster>();
            foreach (Monster m in columns[col])
                if (m != null)
                    if (m.IsAlive())
                        monList.Add(m);
            return monList.ToArray();
        }

        /// <summary>
        /// Return whether a given monster is considered back-row
        /// </summary>
        private bool MonsterIsBackRow(Monster monster)
        {
            int col = GetMonsterColumn(monster);
            
            // Columns 2 and 3 always front rows
            if (col == 2 || col == 3)
                return false;
                
            // If column 3 isn't empty, then 0 and 1 cannot be front rows
            if (!ColumnIsEmpty(3))
                if (col == 0 || col == 1)
                    return true;
                
            if (!ColumnIsEmpty(2))
                if (col == 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Return the column number a given monster is in
        /// </summary>
        private int GetMonsterColumn(Monster monster)
        {
            for (int i = 0; i < columns.Length; i++)
                foreach (Monster mon in columns[i])
                    if (Object.Equals(mon, monster))
                        return i;
            
            return -1;
        }
        
        /// <summary>
        /// Check whether or not a column contains active monsters
        /// </summary>
        private bool ColumnIsEmpty(int col)
        {
            foreach (Monster mon in columns[col])
                if (mon != null)
                    if (mon.IsAlive())
                        return false;

            return true;
        }

        /// <summary>
        /// Return an array of monsters within the scene, including the dead
        /// </summary>
        private Monster[] GetAllMonsters()
        {
            List<Monster> activeList = new List<Monster>();
            foreach (Monster[] col in columns)
                foreach (Monster mon in col)
                    if (mon != null)
                        activeList.Add(mon);
            return activeList.ToArray();
        }
    }
}
