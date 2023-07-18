using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Морской_бой
{
    public partial class Form1 : Form
    {
        private const int mapSize = 10;
        private const int cellSize = 50;
        private const int distanceToSecondField = mapSize + 3;
        private const int deltaY = 2 * cellSize;
        private static int counterOfBotShips = 4 * 1 + 3 * 2 + 2 * 3 + 1 * 4;
        private static int counterOfPlayerShips = 0;
        private int[,] playerMap = new int[mapSize, mapSize];
        private int[,] botMap = new int[mapSize, mapSize];
        private static Button[,] playerButtons = new Button[mapSize, mapSize];
        private static Button[,] botButtons = new Button[mapSize, mapSize];
        private static HashSet<Ship> playerShips = new HashSet<Ship>();
        private static HashSet<Ship> botShips = new HashSet<Ship>();
        Bot bot;
        public static GameStage GameStage { get; set; } = GameStage.ConfigurationShips;

        public Form1()
        {
            InitializeComponent();
            CreateMap();
            bot = new Bot(botMap, playerMap, playerButtons, mapSize);
        }
        private void CreateMap()
        {
            Width = cellSize * (2 * mapSize + 3) + 20;
            Height = cellSize * (mapSize + 5);
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    playerMap[i, j] = 0;
                    botMap[i, j] = 0;

                    Button playerButton = CreateButton(new Point(i * cellSize, (j + 2) * cellSize), new Size(cellSize, cellSize), Color.White);
                    playerButton.Click += Configurate_Ships;
                    playerButtons[i, j] = playerButton; 
                    Button enemyButton = CreateButton(new Point((i + distanceToSecondField) * cellSize, (j + 2) * cellSize), new Size(cellSize, cellSize), Color.White);
                    enemyButton.Click += Player_Shoot;
                    botButtons[i, j] = enemyButton;
                }
                CreateLabel(new Point((mapSize - 3) / 2 * cellSize, cellSize), "Карта игрока");
                CreateLabel(new Point(((distanceToSecondField) + (mapSize - 3) / 2) * cellSize, cellSize), "Карта протвиника");
                Button startGame = CreateButton(new Point(Width / 2 - 2 * cellSize, ((mapSize + 3) * cellSize)), "Начать игру");
                startGame.Click += StartGame_Click;
            }
        }
        public static bool IsInside(int x, int y)
        {
            if (x >= 0 && x < 10 && y >= 0 && y < 10) return true;
            return false;
        }
        private void Player_Shoot(object sender, EventArgs e)
        {
            if (GameStage == GameStage.Playing)
            {
                Button pressedButton = sender as Button;
                if (!Shoot(botMap, pressedButton, out Ship hittenShip)) bot.BotShoot();
            }
        }
        public static bool Shoot(int[,] map, Button pressedButton, out Ship hittenShip)
        {
            hittenShip = null;
            if (GameStage == GameStage.Playing)
            {
                Field field;
                int x;
                int y = (pressedButton.Location.Y - deltaY) / cellSize;
                if (pressedButton.Location.X >= distanceToSecondField * cellSize)
                {
                    x = (pressedButton.Location.X - distanceToSecondField * cellSize) / cellSize;
                    field = Field.Bot;
                }
                else
                {
                    x = pressedButton.Location.X / cellSize;
                    field = Field.Player;
                }
                if (map[x, y] == 0)
                {
                    pressedButton.BackColor = Color.Black;
                    pressedButton.Enabled = false;
                }
                else
                {
                    map[x, y] = 2;
                    pressedButton.BackColor = Color.Red;
                    pressedButton.Enabled = false;
                    CheckIsShipDead(map, new Point(x, y), field, out hittenShip);
                    if (field == Field.Player)
                    {
                        counterOfPlayerShips--;
                        if (counterOfPlayerShips == 0)
                        {
                            GameStage = GameStage.GameOver;
                            MessageBox.Show("Вы проиграли");
                        }
                    }
                    else
                    {
                        counterOfBotShips--;
                        if (counterOfBotShips == 0)
                        {
                            GameStage = GameStage.GameOver;
                            MessageBox.Show("Вы выйграли");
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        private static void CheckIsShipDead(int[,] map, Point point, Field field, out Ship hittenShip)
        {
            HashSet<Ship> ships;
            Button[,] buttons = null;
            hittenShip = null;
            if (field == Field.Player)
            {
                ships = playerShips;
                buttons = playerButtons;
            }
            else
            {
                ships = botShips;
                buttons = botButtons;

            }
            var targetShip = from desiredShip in ships where desiredShip.points.Contains(point) select desiredShip;
            if (targetShip.Count() == 0)
            {
                hittenShip = new Ship(point, map);
                ships.Add(hittenShip);
            }
            else
            {
                hittenShip = targetShip.First();
            }
            if (hittenShip.IsDead(map)) PaintNeighborCells(hittenShip, buttons, map);
        }
        public static void PaintNeighborCells(Ship ship, Button[,] buttons, int[,] map)
        {
            foreach (var point in ship.points)
            {
                for (int i = point.X - 1; i <= point.X + 1; i++)
                {
                    for (int j = point.Y - 1; j <= point.Y + 1; j++)
                    {
                        if (IsInside(i, j) && map[i, j] == 0)
                        {
                            buttons[i, j].BackColor = Color.Black;
                            buttons[i, j].Enabled = false;
                        }
                    }
                }
            }
        }
        private void Configurate_Ships(object sender, EventArgs e)
        {
            if (GameStage == GameStage.ConfigurationShips)
            {
                Button pressedButton = sender as Button;
                int x_coordinate = pressedButton.Location.X / cellSize;
                int y_coordinate = (pressedButton.Location.Y - deltaY) / cellSize;
                if (playerMap[x_coordinate, y_coordinate] == 0)
                {
                    pressedButton.BackColor = Color.Blue;
                    playerMap[x_coordinate, y_coordinate] = 1;
                    counterOfPlayerShips++;
                }
                else
                {
                    pressedButton.BackColor = Color.White;
                    playerMap[x_coordinate, y_coordinate] = 0;
                    counterOfPlayerShips--;
                }
            }
        }
        private void StartGame_Click(object sender, EventArgs e)
        {
            if (counterOfPlayerShips == counterOfBotShips)
            {
                GameStage = GameStage.Playing;
                Button button = sender as Button;
                button.Enabled = false;
            }
            else if (GameStage == GameStage.ConfigurationShips) MessageBox.Show("Вы проставили корабли не по правилам!");
        }
        private Button CreateButton(Point location, Size size, Color color)
        {
            Button button = new Button();
            button.Location = location;
            button.Size = size;
            button.BackColor = color;
            Controls.Add(button);
            return button;
        }
        private Button CreateButton(Point location, string text)
        {
            Button button = new Button();
            button.Location = location;
            button.Text = text;
            button.Font = new Font("Arial", 24);
            button.AutoSize = true;
            button.BackColor = Color.White;
            Controls.Add(button);
            return button;
        }
        private void CreateLabel(Point location, string text)
        {
            Label label = new Label();
            label.Location = location;
            label.Text = text;
            label.Font = new Font("Arial", 24);
            label.AutoSize = true;
            Controls.Add(label);
        }
    }
    enum Field
    {
        Player,
        Bot
    }
}
