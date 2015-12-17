/* San Jose State University
 * CS 134 - Game Design and Programming
 * Instructor: Sun Tee Teoh
 * Student: Andrew Kalenda
 * 
 * A* Algorithm Demo
 * 
 * There is a minor bug where overwriting a start/goal tile with another
 * tile does not remove the start/goal information
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AstarDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int HEIGHT = 16;
        int WIDTH = 16;
        Tile[,] tileAt;
        Brush currColor;
        Int32 currCost;
        Astar astar;
        Boolean pathNotSet;
        Int32 startX;
        Int32 startY;
        Int32 goalX;
        Int32 goalY;
        delegate void PaintFunction(Tile tile);
        PaintFunction extraStep;
        PaintFunction doNothing;
        PaintFunction changeStart;
        PaintFunction changeGoal;
        Astar.DataFeed paintAstar;
        LinkedList<Int32[]> path;

        // MAIN FUNCTION: set up the GUI
        public MainWindow()
        {
            InitializeComponent();

            // Set up the grid
            for (int x = 0; x < WIDTH; x++)
                bttnGrid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int y = 0; y < HEIGHT; y++)
                bttnGrid.RowDefinitions.Add(new RowDefinition());

            // Fill it with buttons
            Thickness noBorder = new Thickness(0);
            tileAt = new Tile[HEIGHT, WIDTH];
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    tileAt[x, y] = new Tile(); 
                    tileAt[x, y].x = x;
                    tileAt[x, y].y = y;
                    tileAt[x, y].BorderThickness = noBorder; 
                    tileAt[x, y].Click += setTile; // hook the paint function below so it is invoked on click
                    bttnGrid.Children.Add(tileAt[x, y]);
                    Grid.SetColumn(tileAt[x, y], x);
                    Grid.SetRow(tileAt[x, y], y);
                }
            }

            // Initialize the pathfinding data
            reset();

            // DELEGATES
            // Set up some functional programming for the paint callbacks
            extraStep = doNothing = delegate(Tile tile) { };
            changeStart = delegate(Tile tile)
            {
                if (startX != -1)
                {
                    tileAt[startX, startY].Background = Brushes.White;
                    astar.updateTraversal(startX, startY, 1);
                }
                startX = tile.x;
                startY = tile.y;
            };
            changeGoal = delegate(Tile tile)
            {
                if (goalX != -1)
                {
                    tileAt[goalX, goalY].Background = Brushes.White;
                    astar.updateTraversal(goalX, goalY, 1);
                }
                goalX = tile.x;
                goalY = tile.y;
            };
            paintAstar = delegate(Int32 x, Int32 y, Char type, Int32 data, Char direction)
            {
                switch (type)
                {
                    case 'o':
                        if (tileAt[x, y].Background.Equals(Brushes.White))
                            tileAt[x, y].Background = Brushes.Beige;
                        if (tileAt[x, y].Background.Equals(Brushes.Blue))
                            tileAt[x, y].Background = Brushes.Teal;
                        tileAt[x, y].Content = direction + " " + data;
                        break;
                    case 'c':
                        if (tileAt[x, y].Background.Equals(Brushes.White))
                            tileAt[x, y].Background = Brushes.DarkGoldenrod;
                        if (tileAt[x, y].Background.Equals(Brushes.Blue))
                            tileAt[x, y].Background = Brushes.DarkOrange;
                        break;
                    default:
                        break;
                }
            };
        }

        // BUTTON FUNCTIONS: reset
        public void reset()
        {
            Int32[,] costFor = new Int32[WIDTH, HEIGHT];
            pathNotSet = true;
            startX = -1;
            startY = -1;
            goalX = -1;
            goalY = -1;

            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    costFor[x, y] = 1;
                    tileAt[x, y].Background = Brushes.White;
                    tileAt[x, y].Content = "";
                }
            }
            astar = new Astar(WIDTH, HEIGHT, costFor);
        }
        public void reset(Object bttn, EventArgs e)
        {
            reset();
        }

        // BUTTON FUNCTIONS: painting tiles
        public void setPaint_Free(Object bttn, EventArgs e)
        {
            currColor = Brushes.White;
            currCost = 1;
            extraStep = doNothing;
        }
        public void setPaint_Goal(Object bttn, EventArgs e)
        {
            currColor = Brushes.Red;
            currCost = 1;
            extraStep = changeGoal;
        }
        public void setPaint_Strt(Object bttn, EventArgs e)
        {
            currColor = Brushes.Green;
            currCost = 1;
            extraStep = changeStart;
        }
        public void setPaint_Wall(Object bttn, EventArgs e)
        {
            currColor = Brushes.DarkGray;
            currCost = Int32.MaxValue;
            extraStep = doNothing;
        }
        public void setPaint_Watr(Object bttn, EventArgs e)
        {
            currColor = Brushes.Blue;
            currCost = 8;
            extraStep = doNothing;
        }
        public void setTile(Object bttn, EventArgs e)
        {
            Tile tile = (Tile)bttn;
            tile.Background = currColor;
            tile.Content = "";
            astar.updateTraversal(tile.x, tile.y, currCost);
            extraStep(tile);
        }

        // PATHING FUNCTIONS
        public void setPath()
        {
            // Have the start and end points not been selected yet?
            if (startX == -1 || goalX == -1)
                return;

            astar.navFromTo(startX, startY, goalX, goalY, paintAstar);
            pathNotSet = false;
        }


        // BUTTON FUNCTIONS: stepping
        public void stepNext(Object bttn, EventArgs e)
        {
            if (pathNotSet)
            {
                setPath();
                return;
            }
            if ((path = astar.step()) != null)
                foreach (Int32[] pair in path)
                    tileAt[pair[0], pair[1]].Background = Brushes.Yellow;
        }
        public void stepFnsh(Object bttn, EventArgs e)
        {
            if (pathNotSet)
            {
                setPath();
                return;
            }
            while ((path = astar.step()) == null) ;
            foreach (Int32[] pair in path)
                tileAt[pair[0], pair[1]].Background = Brushes.Yellow;
        }
        
        // NESTED CONVENIENCE CLASS
        public class Tile : Button
        {
            public Int32 x;
            public Int32 y;
        }
    }
}
