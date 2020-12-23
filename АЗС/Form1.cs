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
        private bool place1, place2;
        private List<Image> images = new List<Image>();
        private List<PictureBox> pictureBoxes = new List<PictureBox>();
        List<Panel> panels = new List<Panel>();
        private bool isOn;
        private Semaphore sem1 = new Semaphore(1, 1);
        private Semaphore sem2 = new Semaphore(1, 1);

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


        private void button1_Click(object sender, EventArgs e)
        {
            if (!isOn)
            {
                var rng = new Random();
                pictureBoxes.ForEach(box => box.BackgroundImage = images[rng.Next(0, pictureBoxes.Count)]);
                isOn = true;
                button1.Text = "Остановить";
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

        void checkPlace()
        {
            if (place1)
            {
                var car = getBox();
                Thread thread = new Thread(() => fillCar(car, "panel1"));
                thread.Start();
            }
            else
            {
                if (place2)
                {
                    var car = getBox();
                    Thread thread = new Thread(() => fillCar(car, "panel2"));
                    thread.Start();
                }
            }
        }

        PictureBox getBox()
        {
            var rng = new Random();
            PictureBox box;
            do
            {
                box = pictureBoxes[rng.Next(0, pictureBoxes.Count)];
            } while (box.BackgroundImage == default);

            return box;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            checkPlace();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            var rng = new Random();
            pictureBoxes.ForEach(box =>
            {
                if (box.BackgroundImage==default)
                {
                    box.BackgroundImage = images[rng.Next(0, images.Count)];
                }
            });
        }

        void fillCar(PictureBox car, string namePlace)
        {
            if (namePlace.Equals("panel1"))
            {
                sem1.WaitOne();
                place1 = false;
                Image image = car.BackgroundImage;
                car.BackgroundImage = default;
                var rng = new Random();
                panel1.BackgroundImage = image;
                Thread.Sleep(rng.Next(1, 6) * 1000);
                panel1.BackgroundImage = default;
                place1 = true;
                sem1.Release();
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