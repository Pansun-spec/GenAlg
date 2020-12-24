using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GenAlg
{
    public partial class Form1 : Form
    {
        static Random rd;
        static int popsize, epoch;
        static double pcross, pmut, xmin, xmax;
        const int lc = 10; //размер хромосомы

        public Form1()
        {
            InitializeComponent();

            //задаем начальные значения
            textBox1.Text = "100";
            textBox2.Text = "0.7";
            textBox3.Text = "0.1";
            textBox4.Text = "10";
            textBox6.Text = "5";
            textBox7.Text = "-5";
            label5.Text = "Функция: f(x) = x^2 + 4;";
            checkBox1.Checked = true; //по-умолчанию целочисленное кодирование

            //инициализация переменных
            rd = new Random(Guid.NewGuid().GetHashCode());
            popsize = int.Parse(textBox1.Text);
            pcross = GetContolText(textBox2);
            pmut = GetContolText(textBox3);
            epoch = int.Parse(textBox4.Text);
            xmax = GetContolText(textBox6);
            xmin = GetContolText(textBox7);
        }

        //исходная функция
        private static double f(double x)
        {
            return x * x + 4.0;
        }

        //выход из приложения
        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //класс особи (целочисленное кодирование)
        public class Indv
        {
            public Boolean[] chrom; //хромосома
            public double fit; //значение фитнесс функции
            public double fen; //фенотип

            public Indv(bool check) //размер хромосомы
            {
                chrom = new Boolean[lc];
                for (int i = 0; i < lc; i++)
                {
                    chrom[i] = coin();
                }
                if (check) //для целочисленного кодирования
                {
                    fen = decode(chrom);
                }
                else //для вещественного
                {
                    fen = rd.Next() % (xmax - xmin) + xmin;
                }
                fit = f(fen);
            }

            public Indv DeepCopy()
            {
                Indv other = (Indv)this.MemberwiseClone();
                other.chrom = new Boolean[lc];
                Array.Copy(chrom, other.chrom, lc);
                other.fen = decode(other.chrom);
                other.fit = f(other.fen);
                return other;
            }
        }

        //бросок монеты (случайность)
        private static Boolean coin(double p = 0.5)
        {
            if (p == 1)
            {
                return true;
            }
            else
                return rd.NextDouble() <= p;
        }

        //декодирование хромосомы
        private static double decode(Boolean[] chrom)
        {
            double ac = 0;
            double pw = 1;
            for (int i = 0; i < chrom.Length; i++)
            {
                if (chrom[i])
                {
                    ac += pw;
                }
                pw *= 2;
            }
            return xmin + (xmax - xmin) * ac / (pw - 1);
        }

        //Отбор
        private static void Shuffle(Indv[] pop)
        {
            for (int i = pop.Length - 1; i > 0; i--)
            {
                int randomIndex = rd.Next(0, i + 1);
                Indv temp = pop[i].DeepCopy();
                pop[i] = pop[randomIndex].DeepCopy();
                pop[randomIndex] = temp.DeepCopy();
            }
        }

        //турнир с k = 2
        private static void Tour(Indv[] pop, ref int pick, ref int idx)
        {
            if (pick >= pop.Length)
            {
                Shuffle(pop); //перемешиваем в случайном порядке
                pick = 0;
            }

            int i1 = pick;
            int i2 = pick + 1;

            //тут можно поменять знак в зависимости от задачи
            pick += 2;
            if (pop[i1].fit < pop[i2].fit)
            {
                idx = i1;
            }
            else
            {
                idx = i2;
            }
        }

        //отбираем популяцию
        private static Indv[] Selection(Indv[] pop)
        {
            int pick = 0;
            int idx = 0;
            List<Indv> nwpop = new List<Indv>();
            for (int i = 0; i < pop.Length; i++)
            {
                Tour(pop, ref pick, ref idx);
                nwpop.Add(pop[idx].DeepCopy());
            }
            return nwpop.ToArray();
        }

        //Скрещивание
        private static void Crossover(Boolean[] c1, Boolean[] c2, Boolean[] p1, Boolean[] p2)
        {
            int k = 0;
            if (coin(pcross))
            {
                k = rd.Next(c1.Length);
            }
            else
            {
                k = c1.Length - 1;
            }

            for (int i = 0; i < k; i++)
            {
                c1[i] = p1[i];
                c2[i] = p2[i];
            }
            for (int i = k; i < c1.Length; i++)
            {
                c1[i] = p2[i];
                c2[i] = p1[i];
            }
        }

        //Мутация
        private static void Mutation(Boolean[] chrom)
        {
            if (coin(pmut))
            {
                int k = rd.Next(chrom.Length);
                chrom[k] = !chrom[k];
            }
        }

        //арифметический кроссинговер
        private static double Crossover(double fenp1,double fenp2)
        {
            if (coin(pcross))
            {
                double w = rd.NextDouble();
                return w * fenp1 + (1.0 - w) * fenp2;
            }
            return fenp1;
        }

        //мутация вещ числа
        private static double Mutation(double fen)
        {
            if (coin(pmut))
            {
                return fen + rd.NextDouble();
            }
            return fen;
        }

        //Генерация нового поколения
        private static Indv[] Generation(Indv[] pop, bool check)
        {
            Indv[] pops = Selection(pop);
            Indv[] newpop = Init(check);
            int i = 0;
            while (true)
            {
                //целочисленное кодирование
                if (check)
                {
                    Crossover(newpop[i].chrom, newpop[i + 1].chrom,
                                 pops[i].chrom, pops[i + 1].chrom);

                    Mutation(newpop[i].chrom);
                    Mutation(newpop[i + 1].chrom);

                    newpop[i].fen = decode(newpop[i].chrom);
                    newpop[i + 1].fen = decode(newpop[i + 1].chrom);
                }
                else //вещественное
                {
                    newpop[i].fen = Crossover(pops[i].fen, pops[i + 1].fen);
                    newpop[i + 1].fen = Crossover(pops[i].fen, pops[i + 1].fen);
                    newpop[i].fen = Mutation(newpop[i].fen);
                    newpop[i + 1].fen = Mutation(newpop[i + 1].fen);
                }

                newpop[i].fit = f(newpop[i].fen);
                newpop[i + 1].fit = f(newpop[i + 1].fen);

                i += 2;
                if (i >= popsize)
                {
                    break;
                }
            }
            return newpop;
        }

        //создание случайной популяции
        private static Indv[] Init(bool check)
        {
            List<Indv> newpop = new List<Indv>();
            for (int i = 0; i < popsize; i++)
            {
                newpop.Add(new Indv(check));
            }
            return newpop.ToArray();
        }

        //получаем минимум фитнесс-функции
        private static double GetMin(Indv[] pop)
        {
            double xm = pop[0].fen;
            double fm = pop[0].fit;
            for (int i = 0; i < popsize; i++)
            {
                if (fm > pop[i].fit)
                {
                    xm = pop[i].fen;
                    fm = pop[i].fit;
                }
            }
            return xm;
        }

        //поиск минимума функции
        private void button2_Click(object sender, EventArgs e)
        {
            //получаем сначала все параметры с формы
            popsize = int.Parse(textBox1.Text);
            pcross = GetContolText(textBox2);
            pmut = GetContolText(textBox3);
            epoch = int.Parse(textBox4.Text);
            xmax = GetContolText(textBox6);
            xmin = GetContolText(textBox7);

            chart1.Series["f(x)"].Points.Clear();
            chart1.Series["gmin"].Points.Clear();

            //основной цикл по эпохам
            Indv[] pop = Init(checkBox1.Checked);

            double gmin = GetMin(pop);

            //для построения графика функции
            double x = 0;
            chart1.Series["gmin"].Points.AddXY(Math.Round(gmin,3), f(gmin));

            for (int i = 0; i < epoch; i++)
            {
                pop = Generation(pop, checkBox1.Checked);
                gmin = GetMin(pop);
                chart1.Series["gmin"].Points.AddXY(Math.Round(gmin,3), f(gmin));
            }

            //сам график
            x = xmin;
            double step = 1;
            while (x <= xmax)
            {
                chart1.Series["f(x)"].Points.AddXY(Math.Round(x,3), f(x));
                x += step;
            }

            textBox5.Text = "Xmin* = " + Math.Round(gmin,3).ToString() + "; Fmin* = " + Math.Round(f(gmin),3).ToString() + ";";
        }

        //получаем значения из формы заменяя при этом точку на запятую
        public double GetContolText(TextBox textBox)
        {
            if (textBox.Text != "")
            {
                return Convert.ToDouble(textBox.Text.Replace(".", ","));
            }
            else
            {
                return 0;
            }
        }
    }
}