using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Algorithm_A
{
    public class GridCounter: IEquatable<GridCounter>
    {
        public GridCounter(Grid mygrid, GridCounter previousGrid, int value, MainWindow gui)
        {
            MyGrid = mygrid;
            foreach(var gr in previousGrid.PathToMyGrid)
                PathToMyGrid.Add(gr);
            PathToMyGrid.Add(previousGrid);
            Count = value + previousGrid.Count;
            gui.Dispatcher.Invoke(() => NumRow = Grid.GetRow(MyGrid));
            gui.Dispatcher.Invoke(() => NumCol = Grid.GetColumn(MyGrid));
        }
        public GridCounter(Grid mygrid)
        {
            MyGrid = mygrid;
            NumRow = Grid.GetRow(MyGrid);
            NumCol = Grid.GetColumn(MyGrid);
        }

        public List<GridCounter> PathToMyGrid = new List<GridCounter>();
        public Grid MyGrid { get; set; }
        public int Count { get; set; }
        public int NumRow { get; set; }
        public int NumCol { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as GridCounter);
        }
        public bool Equals(GridCounter other)
        {
            return (NumRow == other.NumRow && NumCol == other.NumCol);
        }
        public override int GetHashCode()
        {
            return (NumRow.GetHashCode() ^ NumCol.GetHashCode());
        }
    }
}
