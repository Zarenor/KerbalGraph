﻿/* Name:    KerbalGraph (Graph GUI Plugin)
 * Version: 1.3   (KSP 0.22+)
 * Copyright 2014, Michael Ferrara, a.k.a. Ferram4 and Zachary Jordan, a.k.a. Zarenor Darkstalker.
 * 
 * This file is part of KerbalGraph.    
 * KerbalGraph is free software: you can redistribute it and/or modify    
 * it under the terms of the GNU General Public License as published by    
 * the Free Software Foundation, either version 3 of the License, or    
 * (at your option) any later version.
 *     
 * KerbalGraph is distributed in the hope that it will be useful,    
 * but WITHOUT ANY WARRANTY; without even the implied warranty of    
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the    
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License    
 * along with KerbalGraph.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * KerbalGraph is derived from FerramGraph, also released under the GPLv3, and written by Michael Ferrara, a.k.a. Ferram4.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalGraph
{
    public class KerbalGraph
    {
        class KerbalGraphLine
        {
            private Vector4d bounds;
            private Texture2D lineTexture;
            private Texture2D legendTexture;
            public bool displayInLegend;
            private double[] rawDataX = new double[1];
            private double[] rawDataY = new double[1];
            private int[] pixelDataX = new int[1];
            private int[] pixelDataY = new int[1];
            public int lineThickness;
            public Color lineColor = new Color();
            public Color backgroundColor = new Color();
            private double verticalScaling;
            private double horizontalScaling;

            public Texture2D LegendTexture
            {
                get
                {
                    return legendTexture;
                }

                protected set
                {
                    legendTexture = value;
                }
            }

            public Texture2D LineTexture
            {
                get
                {
                    return lineTexture;
                }

                protected set
                {
                    lineTexture = value;
                }
            }

            public Vector4d Bounds
            {
                get
                {
                    return bounds;
                }

                set
                {
                    bounds = value;
                    if(rawDataX.Length > 0)
                    {
                        ConvertRawToPixels();
                    }
                }
            }

            public double VerticalScaling
            {
                get
                {
                    return verticalScaling;
                }

                set
                {
                    verticalScaling = value;
                    ConvertRawToPixels();
                }
            }

            public double HorizontalScaling
            {
                get
                {
                    return horizontalScaling;
                }

                set
                {
                    horizontalScaling = value;
                    ConvertRawToPixels();
                }
            }

            public KerbalGraphLine(int width, int height)
            {
                LineTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
                Bounds = new Vector4(0, 1, 0, 1);
                lineThickness = 1;
                lineColor = Color.red;
                VerticalScaling = 1;
                HorizontalScaling = 1;
            }

            public void InputData(double[] xValues, double[] yValues)
            {
                int elements = xValues.Length;
                rawDataX = new double[elements];
                rawDataY = new double[elements];

                for(int i = 0; i < elements; i++)
                {
                    if(double.IsNaN(xValues[i]))
                    {
                        xValues[i] = 0;
                        MonoBehaviour.print("Warning: NaN in xValues array; value set to zero");
                    }
                    if(double.IsNaN(yValues[i]))
                    {
                        yValues[i] = 0;
                        MonoBehaviour.print("Warning: NaN in yValues array; value set to zero");
                    }
                }

                rawDataX = xValues;
                rawDataY = yValues;
                ConvertRawToPixels();
            }

            /// <summary>
            /// This method taks the given data, and converts it from data space to texture space, 
            /// using the scaling parameters and the size of the bounds
            /// </summary>
            private void ConvertRawToPixels()
            {
                pixelDataX = new int[rawDataX.Length];
                pixelDataY = new int[rawDataY.Length];

                double xScaling = LineTexture.width / (Bounds.y - Bounds.x);
                double yScaling = LineTexture.height / (Bounds.w - Bounds.z);
                double tmpx, tmpy;

                for(int i = 0; i < rawDataX.Length; i++)
                {
                    tmpx = rawDataX[i] * HorizontalScaling;
                    tmpy = rawDataY[i] * VerticalScaling;

                    tmpx -= Bounds.x;
                    tmpx *= xScaling;

                    tmpy -= Bounds.z;
                    tmpy *= yScaling;

                    tmpx = Math.Round(tmpx);
                    tmpy = Math.Round(tmpy);

                    pixelDataX[i] = (int) tmpx;
                    pixelDataY[i] = (int) tmpy;
                }
                Update();
            }

            public void Update()
            {
                ClearLine();
                int lastx = -1;
                int lasty = -1;
                if(lineThickness < 1)
                    lineThickness = 1;

                for(int k = 0; k < pixelDataX.Length; k++)
                {
                    int tmpx = pixelDataX[k];
                    int tmpy = pixelDataY[k];
                    if(lastx >= 0)
                    {
                        int tmpThick = lineThickness - 1;
                        int xstart = Math.Min(tmpx, lastx);
                        int xend = Math.Max(tmpx, lastx);
                        int ystart;
                        int yend;

                        if(xstart == tmpx)
                        {
                            ystart = tmpy;
                            yend = lasty;
                        }
                        else
                        {
                            ystart = lasty;
                            yend = tmpy;
                        }

                        double m = ((double)yend - (double)ystart) / ((double)xend - (double)xstart);
                        if(Math.Abs(m) <= 1 && (xstart != xend))
                        {
                            for(int i = xstart; i < xend; i++)
                                for(int j = -tmpThick; j <= tmpThick; j++)
                                {
                                    int linear = (int)Math.Round(m * (i - xend) + yend);
                                    if((i >= 0 && i <= LineTexture.width) && (linear + j >= 0 && linear + j <= LineTexture.height))
                                        LineTexture.SetPixel(i, linear + j, lineColor);
                                }
                        }
                        else
                        {
                            ystart = Math.Min(tmpy, lasty);
                            yend = Math.Max(tmpy, lasty);

                            if(ystart == tmpy)
                            {
                                xstart = tmpx;
                                xend = lastx;
                            }
                            else
                            {
                                xstart = lastx;
                                xend = tmpx;
                            }

                            m = 1 / m;

                            for(int i = ystart; i < yend; i++)
                                for(int j = -tmpThick; j <= tmpThick; j++)
                                {
                                    int linear = (int)Math.Round(m * (i - yend) + xend);
                                    if((linear + j >= 0 && linear + j <= LineTexture.width) && (i >= 0 && i <= LineTexture.height))
                                        LineTexture.SetPixel(linear + j, i, lineColor);
                                }

                        }
                    }
                    lastx = tmpx;
                    lasty = tmpy;
                }
                LineTexture.Apply();
                UpdateLineLegend();

            }

            private void UpdateLineLegend()
            {
                LegendTexture = new Texture2D(25, 15, TextureFormat.ARGB32, false);
                for(int i = 0; i < LegendTexture.width; i++)
                    for(int j = 0; j < LegendTexture.height; j++)
                    {
                        if(Mathf.Abs((int) (j - (LegendTexture.height / 2f))) < lineThickness)
                            LegendTexture.SetPixel(i, j, lineColor);
                        else
                            LegendTexture.SetPixel(i, j, backgroundColor);
                    }
                LegendTexture.Apply();
            }

            public void ClearTextures()
            {
                GameObject.Destroy(LegendTexture);
                GameObject.Destroy(LineTexture);
                LineTexture = null;
                LegendTexture = null;
            }
            private void ClearLine()
            {
                for(int i = 0; i < LineTexture.width; i++)
                    for(int j = 0; j < LineTexture.height; j++)
                        LineTexture.SetPixel(i, j, new Color(0, 0, 0, 0));
                LineTexture.Apply();
            }

            #region "Data Export"

            public int GetNumDataPoints()
            {
                return rawDataX.Length;
            }

            /// <returns>
            /// XMin, XMax, YMin, YMax
            /// </returns>
            public Vector4d GetExtremeData()
            {
                Vector4d extremes = Vector4d.zero;
                extremes.x = rawDataX.Min();
                extremes.y = rawDataX.Max();
                extremes.z = rawDataY.Min();
                extremes.w = rawDataY.Max();

                return extremes;
            }

            public double[] GetRawDataX()
            {
                return rawDataX;
            }

            public double[] GetRawDataY()
            {
                return rawDataY;
            }

            #endregion
        } //KerbalGraphLine



        protected Texture2D graph;

        /// <summary>
        /// The rectangle the graph is calculated ('drawn') onto. NOT necessarily the same size as displayed.
        /// </summary>
        protected Rect drawRect = new Rect(0, 0, 0, 0);

        /// <summary>
        /// The rectangle the graph is displayed onto.
        /// </summary>
        protected Rect displayRect = new Rect(0, 0, 0, 0);

        private Dictionary<string, KerbalGraphLine> allLines = new Dictionary<string, KerbalGraphLine>();

        private Vector4d bounds;
        public bool autoscale = false;
        public bool displayLegend = true;

        public Color backgroundColor = Color.black;
        public Color gridColor = new Color(0.2f, 0.2f, 0.2f); //new Color(0.42f, 0.35f, 0.11f, 1);
        public Color axisColor = new Color(0.8f, 0.8f, 0.8f); //Color.white;

        private string leftBound;
        private string rightBound;
        private string topBound;
        private string bottomBound;
        public string horizontalLabel = "Axis Label Here";
        public string verticalLabel = "Axis Label Here";
        private Vector2 ScrollView = Vector2.zero;


        #region "Emitters"
        public static KerbalGraph NewGraph(int width, int height)
        {
            return KerbalGraphFactory.instance.EmitGraph(width, height);
        }

        public KerbalGraph NewGraph(int width, int height, double minx, double maxx, double miny, double maxy)
        {
            return KerbalGraphFactory.instance.EmitGraph(width, height, minx, maxx, miny, maxy);
        }
        #endregion

        #region Constructors
        internal KerbalGraph(int width, int height)
        {
            graph = new Texture2D(width, height, TextureFormat.ARGB32, false);
            SetBoundaries(0, 1, 0, 1);
            drawRect = new Rect(1, 1, graph.width, graph.height);
            GridInit();
        }

        internal KerbalGraph(int width, int height, double minx, double maxx, double miny, double maxy)
        {
            graph = new Texture2D(width, height, TextureFormat.ARGB32, false);
            SetBoundaries(minx, maxx, miny, maxy);
            drawRect = new Rect(1, 1, graph.width, graph.height);
            GridInit();
        }
        #endregion

        #region Scaling Functions
        public void SetBoundaries(double minx, double maxx, double miny, double maxy)
        {
            bounds.x = minx;
            bounds.y = maxx;
            bounds.z = miny;
            bounds.w = maxy;
            SetBoundaries(bounds);
        }

        public void SetBoundaries(Vector4d boundaries)
        {
            bounds = boundaries;
            leftBound = bounds.x.ToString();
            rightBound = bounds.y.ToString();
            topBound = bounds.w.ToString();
            bottomBound = bounds.z.ToString();
            foreach(KeyValuePair<string, KerbalGraphLine> pair in allLines)
                pair.Value.Bounds=bounds;
        }


        public void SetGridScaleUsingPixels(int gridWidth, int gridHeight)
        {
            GridInit(gridWidth, gridHeight);
            Update();
        }

        public void SetGridScaleUsingValues(double gridWidth, double gridHeight)
        {
            int pixelWidth, pixelHeight;

            pixelWidth = (int) Math.Round(((gridWidth * drawRect.width) / (bounds.y - bounds.x)));
            pixelHeight = (int) Math.Round(((gridHeight * drawRect.height) / (bounds.w - bounds.z)));

            if(pixelWidth <= 1)
            {
                pixelWidth = 5;
                Debug.Log("Warning! Grid width scale too fine for scaling; picking safe alternative");
            }
            if(pixelHeight <= 1)
            {
                pixelHeight = 5;
                Debug.Log("Warning! Grid height scale too fine for scaling; picking safe alternative");
            }

            SetGridScaleUsingPixels(pixelWidth, pixelHeight);


        }

        public void SetLineVerticalScaling(string lineName, double scaling)
        {
            if(!allLines.ContainsKey(lineName))
            {
                MonoBehaviour.print("Error: No line with that name exists");
                return;
            }
            KerbalGraphLine line;

            allLines.TryGetValue(lineName, out line);

            line.VerticalScaling = scaling;
        }


        public void SetLineHorizontalScaling(string lineName, double scaling)
        {
            if(!allLines.ContainsKey(lineName))
            {
                MonoBehaviour.print("Error: No line with that name exists");
                return;
            }
            KerbalGraphLine line;

            allLines.TryGetValue(lineName, out line);

            line.HorizontalScaling= scaling;
        }

        #endregion

        #region GridInit

        private void GridInit()
        {
            int squareSize = 25;
            GridInit(squareSize, squareSize);
        }


        private void GridInit(int widthSize, int heightSize)
        {

            int horizontalAxis, verticalAxis;

            horizontalAxis = (int) Math.Round(-bounds.x * drawRect.width / (bounds.y - bounds.x));
            verticalAxis = (int) Math.Round(-bounds.z * drawRect.height / (bounds.w - bounds.z));

            for(int i = 0; i < graph.width; i++)
            {
                for(int j = 0; j < graph.height; j++)
                {

                    Color grid = new Color(0.42f, 0.35f, 0.11f, 1);
                    if(i - horizontalAxis == 0 || j - verticalAxis == 0)
                        graph.SetPixel(i, j, axisColor);
                    else if((i - horizontalAxis) % widthSize == 0 || (j - verticalAxis) % heightSize == 0)
                        graph.SetPixel(i, j, gridColor);
                    else
                        graph.SetPixel(i, j, backgroundColor);
                }
            }

            graph.Apply();
        }
        #endregion

        #region Add / Remove Line Functions

        public void AddLine(string lineName)
        {
            if(allLines.ContainsKey(lineName))
            {
                MonoBehaviour.print("Error: A Line with that name already exists");
                return;
            }
            KerbalGraphLine newLine = new KerbalGraphLine((int)drawRect.width, (int)drawRect.height);
            newLine.Bounds=bounds;
            allLines.Add(lineName, newLine);            
            Update();
        }

        public void AddLine(string lineName, double[] xValues, double[] yValues)
        {
            int lineThickness = 1;
            AddLine(lineName, xValues, yValues, lineThickness);
        }

        public void AddLine(string lineName, double[] xValues, double[] yValues, Color lineColor)
        {
            int lineThickness = 1;
            AddLine(lineName, xValues, yValues, lineColor, lineThickness);
        }

        public void AddLine(string lineName, double[] xValues, double[] yValues, int lineThickness)
        {
            Color lineColor = Color.red;
            AddLine(lineName, xValues, yValues, lineColor, lineThickness);
        }

        public void AddLine(string lineName, double[] xValues, double[] yValues, Color lineColor, int lineThickness)
        {
            AddLine(lineName, xValues, yValues, lineColor, lineThickness, true);

        }

        public void AddLine(string lineName, double[] xValues, double[] yValues, Color lineColor, int lineThickness, bool display)
        {
            if(allLines.ContainsKey(lineName))
            {
                MonoBehaviour.print("Error: A Line with that name already exists");
                return;
            }
            if(xValues.Length != yValues.Length)
            {
                MonoBehaviour.print("Error: X and Y value arrays are different lengths");
                return;
            }

            KerbalGraphLine newLine = new KerbalGraphLine((int)drawRect.width, (int)drawRect.height);
            newLine.InputData(xValues, yValues);
            newLine.Bounds=bounds;
            newLine.lineColor = lineColor;
            newLine.lineThickness = lineThickness;
            newLine.backgroundColor = backgroundColor;
            newLine.displayInLegend = display;

            allLines.Add(lineName, newLine);
            Update();
        }

        public void RemoveLine(string lineName)
        {
            if(!allLines.ContainsKey(lineName))
            {
                MonoBehaviour.print("Error: No line with that name exists");
                return;
            }

            KerbalGraphLine line = allLines[lineName];
            allLines.Remove(lineName);

            line.ClearTextures();
            Update();

        }

        public void Clear()
        {
            foreach(KeyValuePair<string, KerbalGraphLine> line in allLines)
            {
                line.Value.ClearTextures();
            }
            allLines.Clear();
            Update();
        }

        #endregion

        #region Update Data Functions

        public void UpdateLineData(string lineName, double[] xValues, double[] yValues)
        {
            if(xValues.Length != yValues.Length)
            {
                MonoBehaviour.print("Error: X and Y value arrays are different lengths");
                return;
            }

            KerbalGraphLine line;

            if(allLines.TryGetValue(lineName, out line))
            {

                line.InputData(xValues, yValues);

                allLines.Remove(lineName);
                allLines.Add(lineName, line);
                Update();
            }
            else
                MonoBehaviour.print("Error: No line with this name exists");

        }

        #endregion


        #region Update Visual Functions
        /// <summary>
        /// Use this to update the graph display
        /// </summary>
        public void Update()
        {
            #region Autoscaling
            if(autoscale)
            {
                Vector4d extremes = Vector4.zero;
                bool init = false;
                foreach(KeyValuePair<string, KerbalGraphLine> pair in allLines)
                {
                    Vector4d tmp = pair.Value.GetExtremeData();

                    if(!init)
                    {
                        extremes.x = tmp.x;
                        extremes.y = tmp.y;
                        extremes.z = tmp.z;
                        extremes.w = tmp.w;
                        init = true;
                    }
                    else
                    {
                        extremes.x = Math.Min(extremes.x, tmp.x);
                        extremes.y = Math.Max(extremes.y, tmp.y);
                        extremes.z = Math.Min(extremes.z, tmp.z);
                        extremes.w = Math.Max(extremes.w, tmp.w);

                    }

                    extremes.x = Math.Floor(extremes.x);
                    extremes.y = Math.Ceiling(extremes.y);
                    extremes.z = Math.Floor(extremes.z);
                    extremes.w = Math.Ceiling(extremes.w);
                }
                SetBoundaries(extremes);
            }
            #endregion
            foreach(KeyValuePair<string, KerbalGraphLine> pair in allLines)
            {
                pair.Value.backgroundColor = backgroundColor;
                pair.Value.Update();
            }

        }

        public void LineColor(string lineName, Color newColor)
        {
            KerbalGraphLine line;
            if(allLines.TryGetValue(lineName, out line))
            {
                line.lineColor = newColor;

                allLines.Remove(lineName);
                allLines.Add(lineName, line);

            }
        }

        public void LineThickness(string lineName, int thickness)
        {
            KerbalGraphLine line;
            if(allLines.TryGetValue(lineName, out line))
            {
                line.lineThickness = Mathf.Clamp(thickness, 1, 6);

                allLines.Remove(lineName);
                allLines.Add(lineName, line);

            }
        }

        #endregion

        #region Data Dump Functions

        /// <summary>
        /// Dumps all lines to a file at a the default GameData/KerbalGraph/ path
        /// </summary>
        /// <param name="fileName">The file name; must include file type: ex: KerbalGraphStuff.csv</param>
        public void DumpDataToCSV(string fileName)
        {
            List<string> linesToDump = new List<string>();
            foreach(KeyValuePair<string, KerbalGraphLine> line in allLines)
            {
                linesToDump.Add(line.Key);
            }
            DumpDataToCSV(fileName, linesToDump);
        }

        /// <summary>
        /// Dumps all lines to a file at a specific path
        /// </summary>
        /// <param name="pathName">The file path; must end with a slash: ex: GameData/KerbalGraph/</param>
        /// <param name="fileName">The file name; must include file type: ex: KerbalGraphStuff.csv</param>
        public void DumpDataToCSV(string pathName, string fileName)
        {
            List<string> linesToDump = new List<string>();
            foreach(KeyValuePair<string, KerbalGraphLine> line in allLines)
            {
                linesToDump.Add(line.Key);
            }
            DumpDataToCSV(pathName, fileName, linesToDump);
        }

        /// <summary>
        /// Dumps a set number of lines to a file at the default GameData/KerbalGraph path
        /// </summary>
        /// <param name="fileName">The file name; must include file type: ex: KerbalGraphStuff.csv</param>
        /// <param name="linesToDump">A list of the lines to dump; is case-sensitive</param>
        public void DumpDataToCSV(string fileName, List<string> linesToDump)
        {
            DumpDataToCSV("GameData/KerbalGraph/", fileName, linesToDump);
        }

        /// <summary>
        /// Dumps a set number of lines to a file at a specific path
        /// </summary>
        /// <param name="pathName">The file path; must end with a slash: ex: GameData/KerbalGraph/</param>
        /// <param name="fileName">The file name; must include file type: ex: KerbalGraphStuff.csv</param>
        /// <param name="linesToDump">A list of the lines to dump; is case-sensitive</param>
        public void DumpDataToCSV(string pathName, string fileName, List<string> linesToDump)
        {
            List<string> columnHeadings = new List<string>();
            List<KerbalGraphLine> linesToPrint = new List<KerbalGraphLine>();

            for(int i = 0; i < linesToDump.Count; i++)
            {
                string lineName = linesToDump[i];
                linesToPrint.Add(allLines[lineName]);
                columnHeadings.Add(horizontalLabel);
                columnHeadings.Add(lineName);
            }

            int maxNumElements = 0;

            for(int i = 0; i < linesToDump.Count; i++)
            {
                KerbalGraphLine line = linesToPrint[i];
                maxNumElements = Math.Max(maxNumElements, line.GetNumDataPoints());
            }

            double[,] dataArray = new double[2 * linesToPrint.Count, maxNumElements];

            int colIndex = 0;

            for(int i = 0; i < linesToDump.Count; i++)
            {
                KerbalGraphLine line = linesToPrint[i];
                double[] rawX = line.GetRawDataX();
                double[] rawY = line.GetRawDataY();

                for(int j = 0; j < rawX.Length; j++)
                {
                    dataArray[colIndex, j] = rawX[j];
                    dataArray[colIndex + 1, j] = rawY[j];
                }
                colIndex += 2;
            }

            string fileNameAndPath = pathName + fileName;

            KerbalGraphIO.WriteToFile(fileNameAndPath, columnHeadings, dataArray);
        }

        #endregion

        /// <summary>
        /// This displays the graph.
        /// </summary>
        public void Display(params GUILayoutOption[] options)
        {
            const int axisDisplaySize = 30;
            const int legendSpacing = 20;

            GUIStyle BackgroundStyle = new GUIStyle(GUI.skin.box);
            BackgroundStyle.hover = BackgroundStyle.active = BackgroundStyle.normal;
            GUIStyle LabelStyle = new GUIStyle(GUI.skin.label);
            LabelStyle.alignment = TextAnchor.UpperCenter;

            //ScrollView = GUILayout.BeginScrollView(ScrollView, false, false, options);
            GUILayout.BeginHorizontal(options);

            //Vertical axis and labels
            GUILayout.BeginVertical(GUILayout.Width(axisDisplaySize), GUILayout.ExpandHeight(true));
            GUILayout.Label(topBound, LabelStyle, GUILayout.Height(20), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.Label(verticalLabel, LabelStyle, GUILayout.Height(100), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.Label(bottomBound, LabelStyle, GUILayout.Height(20), GUILayout.ExpandWidth(true));
            GUILayout.Space(axisDisplaySize); //Endspace to line bottomBound up with axis
            GUILayout.EndVertical();//End Vertical Axis and Labels


            //Graph itself
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            UpdateDisplayRect(GUILayoutUtility.GetLastRect());
            GUI.DrawTexture(displayRect, graph);
            foreach(KeyValuePair<string, KerbalGraphLine> pair in allLines)
                GUI.DrawTexture(displayRect, pair.Value.LineTexture);
            //Debug.Log("[KG] displayRect: " + displayRect);

            //Horizontal Axis and Labels
            GUILayout.BeginHorizontal(GUILayout.Height(axisDisplaySize), GUILayout.ExpandWidth(true));
            GUILayout.Label(leftBound, LabelStyle, GUILayout.Width(20), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.Label(horizontalLabel, LabelStyle, GUILayout.Width(160));
            GUILayout.FlexibleSpace();
            GUILayout.Label(rightBound, LabelStyle, GUILayout.Width(20), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();//End Horizontal Axis and Labels

            GUILayout.EndVertical();//End Graph and Hor Axis

            //Legend Area
            if(displayLegend)
            {
                GUILayout.Space(10);//Seperate from graph
                GUILayout.BeginVertical(GUILayout.Width(60));
                int startingSpace = ((int)displayRect.height - allLines.Count * legendSpacing) / 2;
                GUILayout.Space(startingSpace);
                foreach(KeyValuePair<string, KerbalGraphLine> pair in allLines)
                {
                    if(!pair.Value.displayInLegend)
                        continue;
                    GUILayout.BeginHorizontal(GUILayout.Height(legendSpacing - 5));
                    GUI.DrawTexture(GUILayoutUtility.GetRect(25, legendSpacing - 5), pair.Value.LegendTexture);
                    GUILayout.Label(pair.Key, LabelStyle, GUILayout.Width(35));
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();//End Legend
            }

            GUILayout.EndHorizontal();
            //GUILayout.EndScrollView();

        }

        /// <summary>
        /// This function filters out rectangles unity supplies during the layout process that are invalid, 
        /// so that displayRect only contains valid rectangles.
        /// </summary>
        /// <param name="dr">The rectangle to be tested, and if valid, saved to displayRect and used.</param>
        private void UpdateDisplayRect(Rect dr)
        {
            if((dr.x > 0 || dr.y > 0 || dr.height > 1 || dr.width > 1) && (dr.height > 0 && dr.width > 0))
            {
                displayRect = dr;
            }
        }

    }
}
