using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Algorithm_A
{
    /// <summary>
    /// Интерфейс состоит из сетки гридов в каждом из которых прямоугольник(Rectangle) и textblock
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread Th = null; // поток для работы основного алгоритма
        public static bool ModeWall = false; // переменная, которая определяет включен ли режим добавления стены
        public enum PointPosition { A,B,None }; //набор перечесление для начальной и конечной точки алгоритма
        public static PointPosition position = PointPosition.None; // начальная позиция None
        public GridCounter StartGrid { get; set; } // начальный прямоугольник А
        public GridCounter FinishGrid { get; set; } // конечный прямоугольник В
        public GridCounter FoundPath { get; set; } //прямоугольник, который содержит все прямоугольники от А до В (PathToMyGrid)
        public int CountColumns { get; set; } // количество столбцов с прямоугольниками в программе
        public int CountRows { get; set; } // количество строк с прямоугольниками в программе
        readonly int StraightStep = 10; // стоимость перехода на соседний прямоугольник под прямым углом
        readonly int ObliqueStep = 14; // стоимость перехода на соседний прямоугольник под острым углом

        public List<GridCounter> AllGrids = new List<GridCounter>(); // набор всех прямоугольников
        public List<Grid> GrayWallGrids = new List<Grid>(); // набор серых прямоугольников (стена)
        public List<GridCounter> CounterPassedGrid = new List<GridCounter>(); // набор всех пройденный прямоугольников от точки А до В

        public MainWindow()
        {
            
            InitializeComponent();
            WindowState = WindowState.Maximized; // увеличиваю окно приложения на максимум
            InitRectangles(); // создаю сетку из прямоугольников на форме
        }
        internal void InitRectangles()
        {
            CountRows = MainGrid.RowDefinitions.Count;
            CountColumns = MainGrid.ColumnDefinitions.Count;
            for (int r = 0; r < CountRows; r++)
            {
                for (int c = 0; c < CountColumns; c++)
                {
                    Grid gridpanel = new Grid();
                    gridpanel.Name = $"grid{r}_{c}";
                    Rectangle rectangle = new Rectangle();
                    gridpanel.Children.Add(rectangle); // добавляю в грид прямоугольник

                    TextBlock textBlock = new TextBlock();
                    gridpanel.Children.Add(textBlock); // добавляю в грид textBlock

                    MainGrid.Children.Add(gridpanel); // добавляю в MainGrid грид
                    Grid.SetRow(gridpanel, r); // устанавливаю гриду номер строки
                    Grid.SetColumn(gridpanel, c); // устанавливаю гриду номер столбца
                    gridpanel.MouseLeftButtonDown += GridPanel_MouseLeftButtonDown; // добавляю гриду событие нажатия левой кнопки мыши
                    gridpanel.MouseRightButtonDown += GridPanel_MouseRightButtonDown; // добавляю гриду событие нажатия левой кнопки мыши
                    AllGrids.Add(new GridCounter(gridpanel)); // добавляю грид в набор всех прямоугольников
                }
            }
        }
        /// <summary>
        /// Метод с запуском потока с алгоритмом
        /// </summary>
        internal void Work()
        {
            try
            {
                CounterPassedGrid.Add(new GridCounter(StartGrid.MyGrid, StartGrid, 0, this)); // добавляем стартовый прямоугольник
                DoAlghoritm(new List<GridCounter>() { StartGrid }); // запуск основного алгоритма
            }
            catch(SystemException ex) { }
            finally
            {
                CounterPassedGrid.Clear(); // очищаем набор пройденных прямоугольников
                if (FoundPath == null)
                    MessageBox.Show("Невозможно построить путь!", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    foreach (var grid in FoundPath.PathToMyGrid) // проход по всем прямоугольникам из FoundPath.PathToMyGrid
                    {
                        Rectangle rect = null;
                        Dispatcher.Invoke(() => rect = (Rectangle)grid.MyGrid.Children[0]);
                        Dispatcher.Invoke(() => rect.Fill = Brushes.Green); // красим прямоугольники в зеленый цвет
                    }
            }
        }
        /// <summary>
        /// Основной алгоритм программы
        /// </summary>
        /// <param name="ChildrenGrid"></param>
        internal void DoAlghoritm(List<GridCounter> ChildrenGrid)
        {
            // в начале цикла только один прямоугольник(стартовый)
            // в цикле добавляются элементы, поэтому цикл длится, пока не найдет прямоугольник В или пока не пройдет все видимые прямоугольники от прямоугольника А
            for (int i = 0; i < ChildrenGrid.Count; i++)
            {
                int row = ChildrenGrid[i].NumRow;
                int col = ChildrenGrid[i].NumCol;

                //вниз
                if (row < CountRows - 1)
                {
                    var child = FindNeighbour(row + 1, col, ChildrenGrid[i], StraightStep);
                    if (child != null)
                        ChildrenGrid.Add(child); // добавляем в цикл
                }
                //налево
                if (col > 0)
                {
                    var child = FindNeighbour(row, col - 1, ChildrenGrid[i], StraightStep);
                    if (child != null)
                        ChildrenGrid.Add(child); // добавляем в цикл
                }
                // наверх
                if (row > 0)
                {
                    var child = FindNeighbour(row - 1, col, ChildrenGrid[i], StraightStep);
                    if (child != null)
                        ChildrenGrid.Add(child); // добавляем в цикл
                }
                //направо
                if (col < CountColumns - 1)
                {
                    var child = FindNeighbour(row, col + 1, ChildrenGrid[i], StraightStep);
                    if (child != null)
                        ChildrenGrid.Add(child); // добавляем в цикл
                }
                //вниз и направо
                if (col < CountColumns - 1 && row < CountRows - 1)
                {
                    var child = FindNeighbour(row + 1, col + 1, ChildrenGrid[i], ObliqueStep);
                    if (child != null)
                        ChildrenGrid.Add(child); // добавляем в цикл
                }
                //вниз и налево
                if (col > 0 && row < CountRows - 1)
                {
                    var child = FindNeighbour(row + 1, col - 1, ChildrenGrid[i], ObliqueStep);
                    if (child != null)
                        ChildrenGrid.Add(child); // добавляем в цикл
                }
                //наверх и налево
                if (row > 0 && col > 0)
                {
                    var child = FindNeighbour(row - 1, col - 1, ChildrenGrid[i], ObliqueStep);
                    if (child != null)
                        ChildrenGrid.Add(child); // добавляем в цикл
                }
                //наверх и направо
                if (row > 0 && col < CountColumns - 1)
                {
                    var child = FindNeighbour(row - 1, col + 1, ChildrenGrid[i], ObliqueStep);
                    if (child != null)
                        ChildrenGrid.Add(child); // добавляем в цикл
                }
            }
        }
        /// <summary>
        /// Поиск соседнего прямоугольника
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="previousGrid"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal GridCounter FindNeighbour(int row, int col, GridCounter previousGrid, int value)
        {
            foreach (var childGrid in AllGrids)
            {
                if (childGrid.NumRow == row && childGrid.NumCol == col && !CounterPassedGrid.Contains(childGrid))
                {
                    if (childGrid.NumCol == FinishGrid.NumCol && childGrid.NumRow == FinishGrid.NumRow)
                    {
                        var foundGrid = new GridCounter(childGrid.MyGrid, previousGrid, value, this);
                        FoundPath = foundGrid;
                        Th.Abort();
                    }
                    else
                    {
                        var foundGrid = new GridCounter(childGrid.MyGrid, previousGrid, value, this);
                        CounterPassedGrid.Add(foundGrid);
                        return foundGrid;
                    }
                }
            }
            return null;
        }
        private void GridPanel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var panel = (Grid)sender;
            var rectangle = (Rectangle)panel.Children[0];
            var textblock = (TextBlock)panel.Children[1];
            if (ModeWall)
            {
                if (rectangle.Fill == Brushes.DarkGray)
                {
                    rectangle.Fill = Brushes.LightBlue;
                    GrayWallGrids.Remove(panel);
                }
            }
            else
                DropStartFinish(rectangle, textblock);
        }
        internal void DropStartFinish(Rectangle rectangle, TextBlock textblock)
        {
            if (rectangle.Fill == Brushes.Green)
                switch (textblock.Text)
                {
                    case "A":
                        position = PointPosition.None;
                        textblock.Text = "";
                        StartGrid = null;
                        if (FoundPath != null)
                            DropPath();
                        rectangle.Fill = Brushes.LightBlue;
                        break;
                    case "B":
                        if (position == PointPosition.None)
                            position = PointPosition.None;
                        else
                            position = PointPosition.A;
                        textblock.Text = "";
                        if(FoundPath != null)
                            DropPath();
                        FinishGrid = null;
                        rectangle.Fill = Brushes.LightBlue;
                        break;
                }
        }
        internal void DropPath()
        {
            foreach (var grid in FoundPath.PathToMyGrid)
            {
                if (grid == StartGrid)
                    continue;
                Rectangle rect = null;
                rect = (Rectangle)grid.MyGrid.Children[0];
                rect.Fill = Brushes.LightBlue;
            }
            FoundPath = null;
        }
        private void GridPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid  = (Grid)sender;
            var rectangle = (Rectangle)grid.Children[0];
            var textblock = (TextBlock)grid.Children[1];
            if (ModeWall)
            {
                if (rectangle.Fill == Brushes.LightBlue)
                {
                    rectangle.Fill = Brushes.DarkGray;
                    GrayWallGrids.Add(grid);
                }
            }
            else
            {
                if (rectangle.Fill == Brushes.LightBlue)
                    switch (position)
                    {
                        case PointPosition.None:
                            if (StartGrid != null)
                                break;
                            textblock.Text = "A";
                            StartGrid = new GridCounter(grid);
                            rectangle.Fill = Brushes.Green;
                            position = PointPosition.A;
                            if (StartGrid != null && FinishGrid != null)
                            {
                                foreach (var gr in GrayWallGrids)
                                    CounterPassedGrid.Add(new GridCounter(gr));
                                Th = new Thread(Work);
                                Th.Start();
                            }
                            break;
                        case PointPosition.A:
                            if (FinishGrid != null)
                                break;
                            textblock.Text = "B";
                            FinishGrid = new GridCounter(grid);
                            rectangle.Fill = Brushes.Green;
                            position = PointPosition.B;
                            if (StartGrid != null && FinishGrid != null)
                            {
                                foreach (var gr in GrayWallGrids)
                                    CounterPassedGrid.Add(new GridCounter(gr));
                                Th = new Thread(Work);
                                Th.Start();
                            }
                            break;
                    }
            }

        }

        private void itemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void itemAddGreySquare_Click(object sender, RoutedEventArgs e)
        {
            itemAddGraySquare.IsEnabled = false;
            itemFinishAddGraySquare.IsEnabled = true;
            ModeWall = true;
            if(StartGrid != null)
                DropStartFinish((Rectangle)StartGrid.MyGrid.Children[0], (TextBlock)StartGrid.MyGrid.Children[1]);
            if (FinishGrid != null)
                DropStartFinish((Rectangle)FinishGrid.MyGrid.Children[0], (TextBlock)FinishGrid.MyGrid.Children[1]);
            MessageBox.Show("Чтобы добавить стену нажмите на выбранный прямоугольник.\n" +
            "Чтобы закончить добавление нажмите кнопку \"Завершить добавление стены\" в пункте меню", "Информация",
            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void itemFinishAddGraySquare_Click(object sender, RoutedEventArgs e)
        {
            itemAddGraySquare.IsEnabled = true;
            itemFinishAddGraySquare.IsEnabled = false;
            ModeWall = false;
            MessageBox.Show("Режим добавление стены завершен", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void itemAboutProgramm_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Данная программа предназначена для всех, кто интересуется алгоритмами поиска и их реализацией." +
                "\nАлгоритм обнаруживает доступные и непроходимые области и правильно определяет кратчайший путь от A до B.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
