using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Морской_бой
{
    public enum Direction
    {
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4
    }
    public enum Hit
    {
        FirstHit,
        SecondHit,
        ThirdHit,
        FourHit
    }
    class Bot
    {
        private readonly int mapSize;
        private int firstHitX, firstHitY, secondHitX, secondHitY;
        private ShipDirection direction;
        private Hit hit = Hit.FirstHit;
        private bool isWrongDirection = false;
        private int[,] botMap;
        private int[,] playerMap;
        private Button[,] playerButtons;
        private Random random = new Random();
        public Bot(int[,] botMap, int[,] playerMap, Button[,] playerButtons, int mapSize)
        {
            this.botMap = botMap; 
            this.playerMap = playerMap;
            this.playerButtons = playerButtons;
            this.mapSize = mapSize;
            Configurate_Ships();
        }
        private void TryPutShip(int shipSize)
        {
            bool successfulPlacement = false;

            while (successfulPlacement != true)
            {
                int x_startPosition, y_startPosition;
                do
                {
                    x_startPosition = random.Next(0, mapSize);
                    y_startPosition = random.Next(0, mapSize);
                }
                while (IsNextToTheShip(x_startPosition, y_startPosition));

                Direction direction = (Direction)random.Next(1, 5);
                switch (direction)
                {
                    case Direction.Up:
                        if (y_startPosition + 1 - shipSize >= 0)
                        {
                            for (int i = 0; i < shipSize; i++)
                            {
                                if (IsNextToTheShip(x_startPosition, y_startPosition - i)) goto default;
                            }
                            for (int i = 0; i < shipSize; i++)
                            {
                                botMap[x_startPosition, y_startPosition - i] = 1;
                            }
                            successfulPlacement = true;
                        }
                        break;
                    case Direction.Down:
                        if (y_startPosition - 1 + shipSize < 9)
                        {
                            for (int i = 0; i < shipSize; i++)
                            {
                                if (IsNextToTheShip(x_startPosition, y_startPosition + i)) goto default;
                            }
                            for (int i = 0; i < shipSize; i++)
                            {
                                botMap[x_startPosition, y_startPosition + i] = 1;
                            }
                            successfulPlacement = true;
                        }
                        break;
                    case Direction.Left:
                        if (x_startPosition - shipSize >= 0)
                        {
                            for (int i = 0; i < shipSize; i++)
                            {
                                if (IsNextToTheShip(x_startPosition - i, y_startPosition)) goto default;
                            }
                            for (int i = 0; i < shipSize; i++)
                            {
                                botMap[x_startPosition - i, y_startPosition] = 1;
                            }
                            successfulPlacement = true;
                        }
                        break;
                    case Direction.Right:
                        if (x_startPosition + shipSize < 9)
                        {
                            for (int i = 0; i < shipSize; i++)
                            {
                                if (IsNextToTheShip(x_startPosition + i, y_startPosition)) goto default;
                            }
                            for (int i = 0; i < shipSize; i++)
                            {
                                botMap[x_startPosition + i, y_startPosition] = 1;
                            }
                            successfulPlacement = true;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        private bool IsNextToTheShip(int x, int y)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (Form1.IsInside(x + i, y + j))
                    {
                        if (botMap[x + i, y + j] == 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool AvailableToShoot(int x, int y)
        {
            if (playerButtons[x, y].BackColor == Color.Black || playerButtons[x, y].BackColor == Color.Red) return false;
            return true;
        }
        private void Configurate_Ships()
        {
            for (int numbersOfShips = 4, shipSize = 1; numbersOfShips > 0; numbersOfShips--, shipSize++)
            {
                for (int i = 0; i < numbersOfShips; i++)
                {
                    TryPutShip(shipSize);
                }
            }
        }
        public void BotShoot()
        {
            if (Form1.GameStage == GameStage.Playing)
            {
                System.Threading.Thread.Sleep(250);
                int x, y;
                switch (hit)
                {
                    case Hit.FirstHit:
                        {
                            do
                            {
                                x = random.Next(0, mapSize);
                                y = random.Next(0, mapSize);
                            }
                            while (!AvailableToShoot(x, y));
                            if (Form1.Shoot(playerMap, playerButtons[x, y], out Ship hittenShip))
                            {
                                if (hittenShip.IsDead(playerMap)) BotShoot();
                                else
                                {
                                    firstHitX = x;
                                    firstHitY = y;
                                    hit = Hit.SecondHit;
                                    BotShoot();
                                }
                            }
                        }
                        break;
                    case Hit.SecondHit:
                        {
                            do
                            {
                                x = firstHitX;
                                y = firstHitY;
                                if (random.Next(0, 2) == 0) x = random.Next(firstHitX - 1, firstHitX + 2);
                                else y = random.Next(firstHitY - 1, firstHitY + 2);
                            }
                            while (!(Form1.IsInside(x, y) && AvailableToShoot(x, y)));
                            if (Form1.Shoot(playerMap, playerButtons[x, y], out Ship hittenShip))
                            {
                                if (hittenShip.IsDead(playerMap))
                                {
                                    hit = Hit.FirstHit;
                                    BotShoot();
                                }
                                else
                                {
                                    direction = hittenShip.GetShipDirection();
                                    secondHitX = x;
                                    secondHitY = y;
                                    hit = Hit.ThirdHit;
                                    BotShoot();
                                }
                            }
                        }
                        break;
                    case Hit.ThirdHit:
                        {
                            x = secondHitX;
                            y = secondHitY;
                            switch (direction)
                            {
                                case ShipDirection.Horizontal:
                                    {
                                        if (Form1.IsInside(x + 1, y) && AvailableToShoot(x + 1, y))
                                        {
                                            x += 1;
                                        }
                                        else if (Form1.IsInside(x + 1, y) && playerButtons[x + 1, y].BackColor == Color.Red
                                            && Form1.IsInside(x + 2, y) && AvailableToShoot(x + 2, y))
                                        {
                                            x += 2;
                                        }
                                        else
                                        {
                                            if (Form1.IsInside(x - 1, y) && AvailableToShoot(x - 1, y))
                                            {
                                                x -= 1;
                                            }
                                            else if (Form1.IsInside(x - 1, y) && playerButtons[x - 1, y].BackColor == Color.Red
                                                && Form1.IsInside(x - 2, y) && AvailableToShoot(x - 2, y))
                                            {
                                                x -= 2;
                                            }
                                        }
                                    }
                                    break;
                                case ShipDirection.Vertical:
                                    {
                                        if (Form1.IsInside(x, y + 1) && AvailableToShoot(x, y + 1))
                                        {
                                            y += 1;
                                        }
                                        else if (Form1.IsInside(x, y + 1) && playerButtons[x, y + 1].BackColor == Color.Red
                                            && Form1.IsInside(x, y + 2) && AvailableToShoot(x, y + 2))
                                        {
                                            y += 2;
                                        }
                                        else
                                        {
                                            if (Form1.IsInside(x, y - 1) && AvailableToShoot(x, y - 1))
                                            {
                                                y -= 1;
                                            }
                                            else if (playerButtons[x, y - 1].BackColor == Color.Red && Form1.IsInside(x, y - 2)
                                            && Form1.IsInside(x, y - 2) && AvailableToShoot(x, y - 2))
                                            {
                                                y -= 2;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            if (Form1.Shoot(playerMap, playerButtons[x, y], out Ship hittenShip))
                            {
                                if (hittenShip.IsDead(playerMap))
                                {
                                    hit = Hit.FirstHit;
                                    BotShoot();
                                }
                                else
                                {
                                    hit = Hit.FourHit;
                                    BotShoot();
                                }
                            }
                        }
                        break;
                    case Hit.FourHit:
                        {
                            switch (direction)
                            {
                                case ShipDirection.Horizontal:
                                    {
                                        if (isWrongDirection)
                                        {
                                            secondHitX = firstHitX;
                                            isWrongDirection = false;
                                        }
                                        else isWrongDirection = true;
                                    }
                                    break;
                                case ShipDirection.Vertical:
                                    {
                                        if (isWrongDirection)
                                        {

                                            secondHitY = firstHitY;
                                            isWrongDirection = false;
                                        }
                                        else isWrongDirection = true;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            hit = Hit.ThirdHit;
                            BotShoot();
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
