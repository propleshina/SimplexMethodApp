using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace SimplexMethod
{
    public partial class MainWindow : Window
    {
        private int numVars;
        private int numConstraints;
        private List<TextBox> constraintInputs = new List<TextBox>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtNumVars.Text, out numVars) && int.TryParse(txtNumConstraints.Text, out numConstraints))
            {
                InputsPanel.Visibility = Visibility.Visible;
                Zagruzka.Visibility = Visibility.Visible;
                ConstraintsInputs.Items.Clear();
                constraintInputs.Clear();
                for (int i = 0; i < numConstraints; i++)
                {
                    var constraintInput = new TextBox { Width = 300, Margin = new Thickness(0, 5, 0, 0) };
                    ConstraintsInputs.Items.Add(constraintInput);
                    constraintInputs.Add(constraintInput);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректные числа.");
            }
        }

        private void AddConstraintButton_Click(object sender, RoutedEventArgs e)
        {
            var constraintInput = new TextBox { Width = 300, Margin = new Thickness(0, 5, 0, 0) };
            ConstraintsInputs.Items.Add(constraintInput);
            constraintInputs.Add(constraintInput);
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            double[,] tableau = new double[numConstraints + 1, numVars + numConstraints + 1];

            string[] objectiveCoefficients = txtObjectiveCoefficients.Text.Split(' ');
            for (int j = 0; j < numVars; j++)
            {
                tableau[numConstraints, j] = double.Parse(objectiveCoefficients[j]);
            }

            for (int i = 0; i < numConstraints; i++)
            {
                string[] constraintInput = constraintInputs[i].Text.Split(' ');
                for (int j = 0; j < numVars; j++)
                {
                    tableau[i, j] = double.Parse(constraintInput[j]);
                }
                tableau[i, numVars + i] = 1; 
                tableau[i, numVars + numConstraints] = double.Parse(constraintInput[numVars]);
            }

            for (int j = 0; j < numVars; j++)
            {
                tableau[numConstraints, j] *= -1; 
            }

            string result = SimplexMethod(tableau);
            ResultsTextBlock.Text = result;

            Vigruzka.Visibility = Visibility.Visible;
        }

        public static string SimplexMethod(double[,] tableau)
        {
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);
            string result = "";

            while (true)
            {
                int pivotCol = -1;
                for (int j = 0; j < numCols - 1; j++)
                {
                    if (tableau[numRows - 1, j] < 0)
                    {
                        pivotCol = j;
                        break;
                    }
                }

                if (pivotCol == -1)
                    break;

                int pivotRow = -1;
                double minRatio = double.MaxValue;

                for (int i = 0; i < numRows - 1; i++)
                {
                    if (tableau[i, pivotCol] > 0)
                    {
                        double ratio = tableau[i, numCols - 1] / tableau[i, pivotCol];
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            pivotRow = i;
                        }
                    }
                }

                if (pivotRow == -1)
                {
                    return "Неограниченное решение.";
                }

                double pivotValue = tableau[pivotRow, pivotCol];
                for (int j = 0; j < numCols; j++)
                    tableau[pivotRow, j] /= pivotValue;

                for (int i = 0; i < numRows; i++)
                {
                    if (i != pivotRow)
                    {
                        double factor = tableau[i, pivotCol];
                        for (int j = 0; j < numCols; j++)
                            tableau[i, j] -= factor * tableau[pivotRow, j];
                    }
                }
            }

            result += "Оптимальное решение:\n";
            for (int i = 0; i < numRows - 1; i++)
            {
                result += $"x{i + 1} = {tableau[i, numCols - 1]:F2}\n";
            }
            result += $"Максимальное значение Z = {tableau[numRows - 1, numCols - 1]:F2}\n";

            result += "\nФинальная таблица симплекс-метода:\n";
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    result += $"{tableau[i, j]:F2}\t";
                }
                result += "\n";
            }

            return result;
        }

        private void Zagruzka_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Задача";
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string[] lines = File.ReadAllLines(dlg.FileName);

                txtNumVars.Text = lines[0]; // Количество переменных
                txtNumConstraints.Text = lines[1]; // Количество ограничений
                txtObjectiveCoefficients.Text = lines[2]; // Коэффициенты целевой функции

                for (int i = 0; i < numConstraints; i++)
                {
                    if (i + 3 < lines.Length) // Ограничения начинаются с третьей строки
                    {
                        constraintInputs[i].Text = lines[i + 3];
                    }
                }
            }
        }

        private void Vigruzka_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Решение";
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                File.WriteAllText(dlg.FileName, ResultsTextBlock.Text);
            }
        }
    }
}
