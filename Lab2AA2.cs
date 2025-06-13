/*
Ant žaidimo lentos (1 x n) langelių surašyti atsitiktiniai teigiami skaičiai (taškai). Pradėjęs
pirmame langelyje, žaidėjas vieno ėjimo metu gali pasirinkti – pereiti į kitą langelį (1→2) ir gauti
tiek taškų, kiek yra tame langelyje, ar peršokti du langelius (1→4) ir gauti du kartus daugiau
taškų, nei yra užrašyta tame langelyje. Kokia yra mažiausia taškų suma, kurią žaidėjas gali
surinkti paskutiniame langelyje?
*/
using System.Diagnostics;

namespace Lab2AA2
{
    class Program
    {
        static void Main(string[] args)
        {
            //int curr = 10;
            //int counter;
            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            //Random random = new Random(); //Reikalingas atsitiktiniam tašku generavimui
            //for (int i = 0; i < 6; i++)
            //{
            //    counter = 0;
            //    int[] arr = new int[curr];
            //    for(int j = 0; j < curr; j++)
            //    {
            //        arr[i] = random.Next(1,1000);
            //    }
            //    watch.Stop();
            //    var elapsedMs = watch.ElapsedMilliseconds;
            //    Console.WriteLine( "N =" + watch.Elapsed + " Iteracijus skaicius " + methodToAnalysis(arr, counter));
            //    curr *= 2;
            //}
            int[] board = { 0, 6, 10, 1, 2, 6, 11, 8, 3, 3, 20}; //Ats 46; 0, -, -, (1*2), 2, -, -, (8*2), 3, 3, 20
            int points = 0;
            //int[] board = { 0, 10, 20, 100, 300, 10, 20, 5, 6 }; //Ats 62; 0, 10, 20, -, -, (10*2), -, -, (6*2) 
            //int[] board = new int[10];
            int[] dp = new int[board.Length]; //Laikomas minimalus taškų skaičius kiekviename ejime
            //Random random = new Random(); //Reikalingas atsitiktiniam tašku generavimui
            //for (int i = 1; i < board.Length; i++)
            //{
               // board[i] = random.Next(1, 100);
           // }
            Console.WriteLine("");
            Console.WriteLine("Užduoties sprendimas rekursiniu būdu: " + MinScoreRecursive(board, 0, dp, 0));
            Console.WriteLine("Užduoties sprendimas dinaminio programavimo būdu: " + MinScoreDynamic(board));

        }

        public static int MinScoreRecursive(int[] board, int position, int[] dp, int dpPos)
        {
            int n = board.Length - 1;  //langelių skaičius
            Boolean canJump = true;

            if (position >= n - 2) canJump = false;

            if (canJump == true)
            {
                if (board[position + 1] <= board[position + 3] * 2)
                {
                    int tempPos = board[position + 1] + board[position + 2];
                    if (board[position + 3] * 2 <= tempPos)
                    {
                        dp[dpPos] = board[position + 3] * 2;
                        position += 3;
                        dpPos++;
                        MinScoreRecursive(board, position, dp, dpPos);
                    }
                    else
                    {
                        dp[dpPos] = board[position + 1];
                        position += 1;
                        dpPos++;
                        MinScoreRecursive(board, position, dp, dpPos);
                    }
                }
                else if (board[position + 1] > board[position + 3] * 2)
                {
                    dp[dpPos] = board[position + 3] * 2;
                    position += 3;
                    dpPos++;
                    MinScoreRecursive(board, position, dp, dpPos);
                }
            }
            else if (canJump == false)
            {
                if (position != n && position == n - 2)
                {
                    dp[dpPos] = board[position + 1];
                    dp[dpPos + 1] = board[position + 2];
                    return dp.Sum();
                }
                else if (position != n && position == n - 1)
                {
                    dp[dpPos] = board[position + 1];
                    return dp.Sum();
                }
                else return dp.Sum();
            }
            return dp.Sum();
        }

        public static int MinScoreDynamic(int[] board)
        {
            int n = board.Length - 1;
            int[] dp = new int[n];
            Boolean canJump = true;
            int dpPos = 0;

            for (int i = 0; i < n; i++)
            {
                if (i >= n - 2) canJump = false;
                if (canJump == true)
                {
                    if (board[i + 1] <= board[i + 3] * 2)
                    {
                        int tempPos = board[i + 1] + board[i + 2];
                        if (board[i + 3] * 2 <= tempPos)
                        {
                            dp[dpPos] = board[i + 3] * 2;
                            i += 2;
                            dpPos++;
                        }
                        else
                        {
                            dp[dpPos] = board[i + 1];
                            dpPos++;
                        }
                    }
                    else if (board[i + 1] > board[i + 3] * 2)
                    {
                        dp[dpPos] = board[i + 3] * 2;
                        i += 2;
                        dpPos++;
                    }
                }
                else if (canJump == false)
                {
                    if (i != n && i == n - 2)
                    {
                        dp[dpPos] = board[i + 1];
                        dp[dpPos + 1] = board[i + 2];
                        return dp.Sum();
                    }
                    else if (i != n && i == n - 1)
                    {
                        dp[dpPos] = board[i + 1];
                        return dp.Sum();
                    }
                }
            }
            return dp.Sum();
        }

        public static long methodToAnalysis(int[] arr, int counter)
        {
            long n = arr.Length;
            long k = n;
            for (int i = 0; i < n; i++)
            {
                if (arr[i] / 7 == 0)
                {
                    k -= 2;
                }
                else
                {
                    k += 3;
                }
            }
            if (arr[0] > 0)
            {
                for (int i = 0; i < n * n; i++)
                {
                    if (arr[0] > 0)
                    {
                        k += 3;
                    }
                }
            }
            return counter;
        }

        public static long methodToAnalysis(int n, int[] arr)
        {
            long k = 0;
            Random randNum = new Random();
            for (int i = 0; i < n; i++)
            {
                k += arr[i] + FF3(i, arr);
            }
            return k;
        }

        public static long FF3(int n, int[] arr)
        {
            if (n > 0 && arr.Length > n && arr[n] > 0)
            {
                return FF3(n - 1, arr) + FF3(n - 3, arr);
            }
            return n;
        }
    }
}