using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace АЗС
{
    public partial class Form1 : Form
    {
        private bool place1, place2;//Флаги свободности колонки
        private List<Image> images = new List<Image>();//Список с картинками машин
        private List<PictureBox> pictureBoxes = new List<PictureBox>();//Список пикчер боксов с формы
        List<Panel> panels = new List<Panel>();//Список колонок
        private bool isOn;//Включена/выключена заправка
        private Semaphore sem1 = new Semaphore(1, 1);//Семафор для первой колонки
        private Semaphore sem2 = new Semaphore(1, 1);//Семафор для второй колонки
        private int timeNextCar = 1000;

        public Form1()
        {
            InitializeComponent();
            place1 = true;
            place2 = true;
            foreach (Control control in this.Controls)
            {
                if (control is PictureBox)
                {
                    var box = control as PictureBox;
                    images.Add(box.BackgroundImage);
                    pictureBoxes.Add(box);
                }

                if (control is Panel)
                {
                    panels.Add(control as Panel);
                }
            }

            pictureBoxes.ForEach(box => box.BackgroundImage = default);
        }


        private void button1_Click(object sender, EventArgs e)//Запуск АЗС
        {
            if (!isOn)
            {
                var rng = new Random();
                pictureBoxes.ForEach(box => box.BackgroundImage = images[rng.Next(0, pictureBoxes.Count)]);//Задаем первичную очередь из машин
                isOn = true;
                button1.Text = "Остановить";
                timer2.Interval = timeNextCar;
                timer1.Enabled = true;
                timer2.Enabled = true;
            }
            else
            {
                isOn = false;
                button1.Text = "Запустить";
                timer1.Enabled = false;
                timer2.Enabled = false;
            }
        }

        void checkPlace()//Проверка того есть ли свободные колонки для заправки и если есть отправка туда машины
        {
            if (place1)
            {
                var car = getBox();
                Thread thread = new Thread(() => fillCar(car, "panel1"));//Запуск заправки в отдельном потоке
                thread.Start();
            }
            else
            {
                if (place2)
                {
                    var car = getBox();
                    Thread thread = new Thread(() => fillCar(car, "panel2"));//Запуск заправки в отдельном потоке
                    thread.Start();
                }
            }
        }

        PictureBox getBox()//Вызов случайной машины для заправки
        {
            var rng = new Random();
            PictureBox box;
            do
            {
                box = pictureBoxes[rng.Next(0, pictureBoxes.Count)];
            } while (box.BackgroundImage == default);

            return box;
        }

        private void timer1_Tick(object sender, EventArgs e)//Таймер который проверяет наличие свободного места на заправке раз в 100мс
        {
            checkPlace();
        }

        private void timer2_Tick(object sender, EventArgs e)//Таймер заполняющий очередь раз в 100мс
        {
            var rng = new Random();
            pictureBoxes.ForEach(box =>
            {
                if (box.BackgroundImage==default)
                {
                    box.BackgroundImage = images[rng.Next(0, images.Count)];
                }
            });
            timeNextCar = new Random().Next(100, 3000);
            timer2.Interval = timeNextCar;
        }

        void fillCar(PictureBox car, string namePlace)
        {
            if (namePlace.Equals("panel1"))//Если машина попала на первую заправку
            {
                sem1.WaitOne();//Ожидание освобождения
                place1 = false;//Установка флага занятости
                Image image = car.BackgroundImage;//Получение картинки
                car.BackgroundImage = default;//Установка дефолтной картинки на место машины в очереди
                var rng = new Random();
                panel1.BackgroundImage = image;
                Thread.Sleep(rng.Next(1, 6) * 1000);//Генерация рандомного времени заправки и "Усыпление" потока
                panel1.BackgroundImage = default;//После окончания заправки удаляем машину с колонки
                place1 = true;//Установка флага свободности колонки
                sem1.Release();//Выходим из семафора, чтобы другой мог его использовать
            }
            if (namePlace.Equals("panel2"))
            {
                sem2.WaitOne();
                place2 = false;
                Image image = car.BackgroundImage;
                car.BackgroundImage = default;
                var rng = new Random();
                panel2.BackgroundImage = image;
                Thread.Sleep(rng.Next(1, 6) * 1000);
                panel2.BackgroundImage = default;
                place2 = true;
                sem2.Release();
            }
        }
    }
}