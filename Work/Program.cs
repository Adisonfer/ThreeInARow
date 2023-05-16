using System;
using System.Threading;

namespace Work
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Run();
        }
    }

    public class Board
    {
        private const int _size = 10;
        private char[,] _board; // поле
        private char[,] _tempBoard; // копия поля
        private Random _rand;

        public Board() {
            _rand = new Random();
            _board = new char[_size, _size];
            _tempBoard = new char[_size, _size];   
            Init();
        }

        // Функция создания поля
        public void Init()
        {
            for (int row = 0; row < _board.GetLength(0); row++)
            {
                for (int col = 0; col < _board.GetLength(1); col++)
                {
                    char crystal = GetRandomCrystal(_rand);
                    while (HasMatchedNeighbours(_board, row, col, crystal))
                    {
                        crystal = GetRandomCrystal(_rand);
                    }
                    _board[row, col] = crystal;
                }
            }

        }
        // Функция, возвращающая случайный кристалл
        private char GetRandomCrystal(Random rand)
        {
            int randNum = rand.Next(1, 7);
            return (char)('A' + randNum - 1);
        }
        // Функция, проверяющая наличие соседних кристаллов (символов) с тем же символом
        private bool HasMatchedNeighbours(char[,] board, int row, int col, char crystal)
        {
            if (row > 1 && board[row - 1, col] == crystal && board[row - 2, col] == crystal)
            {
                return true;
            }
            if (col > 1 && board[row, col - 1] == crystal && board[row, col - 2] == crystal)
            {
                return true;
            }
            return false;
        }

        public void Tick()
        {
            Dump();
            checkCombinations(_board); // проверяем существование рядов 3 и более. Если true то удаляем сразу

            _tempBoard = (char[,])_board.Clone(); // с каждым ходом создаем временную копию поля для проверки на сущ ходов

            existenceCombinations(_tempBoard); // проверяем существование ходов
            //реализуем падение кристаллов
            bool flag_map = map_is_complete();
            while (!flag_map) // пока карта будет не вся заполнена
            {
                Thread.Sleep(1000);
                Dump();
                for (int row = 0; row < _board.GetLength(0); row++)
                {
                    for (int col = 0; col < _board.GetLength(1); col++)
                    {
                        // проверка на то, что в 0 строке удален кристалл
                        if (row == 0 && _board[0, col] == ' ')
                        {
                            _board[row, col] = GetRandomCrystal(_rand); // добавляем рандомный
                        }
                        else
                        {
                            if (_board[row, col] == ' ')
                            {
                                char tmp = _board[row, col];
                                _board[row, col] = _board[row - 1, col];
                                _board[row - 1, col] = tmp;
                            }
                        }
                    }
                }
                checkCombinations(_board);
                flag_map = map_is_complete();
            }
            checkCombinations(_board);
        }

        private bool map_is_complete() // метод проверки на заполненность карты
        {
            for (int row = 0; row < _board.GetLength(0); row++)
            {
                for (int col = 0; col < _board.GetLength(1); col++)
                {
                    if (_board[row, col] == ' ')
                        return false;
                }
            }
            return true;
        } 

        private bool CanCrystalMove(int x1, int y1, int x2, int y2)
        {
            for (int row = 0; row < _board.GetLength(0); row++)
            {
                for (int col = 0; col < _board.GetLength(1); col++)
                {
                    _tempBoard[row, col] = _board[row, col];
                }
            }

            char tmp = _tempBoard[y1, x1];
            _tempBoard[y1, x1] = _tempBoard[y2, x2];
            _tempBoard[y2, x2] = tmp;

            // Проверяем, есть ли на поле ряды кристаллов.
            bool hasRows = checkCombinations(_tempBoard);

            return hasRows;
        }

        private bool checkCombinations(char[,] board)
        {
            bool Found = false;
            for (int row = 0; row < board.GetLength(0); row++)
            {
                int countCombinationsRow = 1;
                for (int col = 1; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == board[row, col - 1])
                    {
                        countCombinationsRow++;
                    }
                    else
                    {
                        if (countCombinationsRow >= 3)
                        {
                            Found = true;
                            for (int k = col - countCombinationsRow; k < col; k++)
                            {
                                board[row, k] = ' ';
                            }
                        }
                        countCombinationsRow = 1;
                    }
                }
                // Проверка последних элементов строки
                if (countCombinationsRow >= 3)
                {
                    Found = true;
                    for (int k = board.GetLength(1) - countCombinationsRow; k < board.GetLength(1); k++)
                    {
                        board[row, k] = ' ';
                    }
                }
            }

            for (int col = 0; col < board.GetLength(1); col++)
            {
                int countCombinationCol = 1;
                for (int row = 1; row < board.GetLength(0); row++)
                {
                    if (board[row, col] == board[row - 1, col])
                    {
                        countCombinationCol += 1;
                    }
                    else
                    {
                        if (countCombinationCol >= 3)
                        {
                            Found = true;
                            for (int k = row - countCombinationCol; k < row; k++)
                            {
                                board[k, col] = ' ';
                            }
                        }

                        countCombinationCol = 1;
                    }
                }
                // Проверка последних элементов столбца
                if (countCombinationCol >= 3)
                {
                    Found = true;
                    for (int k = board.GetLength(0) - countCombinationCol; k < board.GetLength(0); k++)
                    {
                        board[k, col] = ' ';
                    }
                }
            }
            return Found;
        }

        private bool existenceCombinations(char[,] board)
        {
            bool found = false;
            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int col = 0; col < board.GetLength(1); col++)
                {
                    char curr = board[row, col]; // сохраняем значение клетки

                    // проходимся по ближайшим клеткам
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (i == 0 && j == 0) continue; // пропускаем текущую клетку
                                                            // проверяем, что соседняя клетка находится в пределах поля
                            int r = row + i;
                            int c = col + j;

                            if (r < 0 || r >= board.GetLength(0) || c < 0 || c >= board.GetLength(1)) continue;

                            // меняем символы во временной копии поля
                            board[row, col] = board[r, c];
                            board[r, c] = curr;

                            // проверяем, есть ли на поле комбинации кристаллов на временной копии поля
                            if (checkCombinations(board))
                            {
                                // комбинация найдена
                                return true;
                            }

                            // возвращаем символы на исходные места во временной копии поля
                            board[r, c] = board[row, col];
                            board[row, col] = curr;
                        }
                    }
                }
            }
            // если дошли до конца и не было найдено комбинаций, то перемешиваем поле и повторяем проверку
            Mix();
            return found;
        }

        public void Mix()
        {
            // перемешиваем игровое поле
            // Создаем временный массив и заполняем его копиями кристаллов из игрового поля

            for (int row = 0; row < _board.GetLength(0); row++)
            {
                for (int col = 0; col < _board.GetLength(1); col++)
                {
                    _tempBoard[row, col] = _board[row, col];
                }
            }
            // Проходимся по каждому элементу игрового поля и заменяем его на случайный кристалл из временного массива

            for (int row = 0; row < _board.GetLength(0); row++)
            {
                for (int col = 0; col < _board.GetLength(1); col++)
                {
                    int randomRow = _rand.Next(0, _board.GetLength(0));
                    int randomCol = _rand.Next(0, _board.GetLength(1));
                    _board[row, col] = _tempBoard[randomRow, randomCol];
                }
            }
        }

        public void Move(int x1, int y1, int x2, int y2)
        {
            if (CanCrystalMove(x1, y1, x2, y2))
            {
                for (int row = 0; row < _tempBoard.GetLength(0); row++)
                {
                    for (int col = 0; col < _tempBoard.GetLength(1); col++)
                    {
                        _board[row, col] = _tempBoard[row, col];
                    }
                }
            }
        }
        // Функция отрисовки
        public void Dump()
        {
            Console.Clear();
            Console.Write("  ");
            for (int row = 0; row < _board.GetLength(0); row++)
            {
                Console.Write(row + " ");
            }
            Console.WriteLine();
            for (int row = 0; row < _board.GetLength(0); row++)
            {
                Console.Write(row + " ");
                for (int col = 0; col < _board.GetLength(1); col++)
                {
                    switch (_board[row, col])
                    {
                        case 'A':
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(_board[row, col] + " ");
                            break;
                        case 'B':
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(_board[row, col] + " ");
                            break;
                        case 'C':
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(_board[row, col] + " ");
                            break;
                        case 'D':
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(_board[row, col] + " ");
                            break;
                        case 'E':
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write(_board[row, col] + " ");
                            break;
                        case 'F':
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write(_board[row, col] + " ");
                            break;
                        case ' ':
                            Console.ResetColor();
                            Console.Write(_board[row, col] + " ");
                            break;
                    }
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }
    }

    public class Player
    {
        public void move(Board board, int x1, int y1, int x2, int y2)
        {
            board.Move(x1, y1, x2, y2);
        }
        public string getUserInput()
        {
            Console.Write("Введите ход (например, 'm 0 1 l' или 'q' для выхода): ");
            return Console.ReadLine();
        }
    }

    public class Game
    {
        private Board board = new Board();
        private Player player = new Player();

        private bool correctionInput(int x1, int y1) {
            if (x1 < 0 || y1 < 0 || x1 > 9 || y1 > 9)
            {
                return false;
            }
            else return true;
        }

        public void Run()
        {
            board.Dump();
            while (true)
            {
                string input = player.getUserInput();
                if (input == "q") {
                    break;
                }
                else
                {
                    int x1, y1;

                    bool resultx1 = int.TryParse(input.Split(' ')[1], out x1);
                    bool resulty1 = int.TryParse(input.Split(' ')[2], out y1);
                    while (!resultx1 || !resulty1)
                    {
                        Console.WriteLine("Неправильно введены данные");
                        input = player.getUserInput();
                        resultx1 = int.TryParse(input.Split(' ')[1], out x1);
                        resulty1 = int.TryParse(input.Split(' ')[2], out y1);
                    }
                    while (!correctionInput(x1, y1))
                    {
                        Console.WriteLine("Выход за границы массива");
                        input = player.getUserInput();
                        x1 = int.Parse(input.Split(' ')[1]);
                        y1 = int.Parse(input.Split(' ')[2]);
                        correctionInput(x1, y1);
                    }

                    int x2 = x1, y2 = y1;
                    switch (input.Split(' ')[3])
                    {
                        case "l":
                             if (x1 > 0) x2--;
                            break;
                        case "r":
                            if (x1 < 9) x2++;
                            break;
                        case "u":
                            if (y1 > 0) y2--;
                            break;
                        case "d":
                            if (y1 < 9) y2++;
                            break;
                        default: Console.WriteLine("Неправильно введено направление"); 
                            break;
                    }
                    player.move(board, x1, y1, x2, y2); 
                    board.Tick();
                    board.Dump();
                }
            }
            Console.WriteLine("Игра завершилась");
        }
    }
}
