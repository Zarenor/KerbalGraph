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
using System.IO;
using System.Text;

namespace KerbalGraph
{
    /// <summary>
    /// Handles IO for the main KerbalGraph class
    /// </summary>
    public static class KerbalGraphIO
    {
        /// <summary>
        /// Writes data to specified file
        /// </summary>
        /// <param name="fileNameAndPath">The path (inside of the KSPRootPath and filename written to</param>
        /// <param name="columnHeadings">The strings at the top of each data column for labeling data</param>
        /// <param name="dataColumnRow">A two-dimensional array, organized so that first index specifies which data variable while the other goes through all available points of that data</param>
        public static void WriteToFile(string fileNameAndPath, List<string> columnHeadings, double[,] dataColumnRow)
        {
            //Open the filestream
            FileStream fs = File.Open(KSPUtil.ApplicationRootPath.Replace("\\", "/") + fileNameAndPath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

            //Write the column headings
            string tmp_s = "";
            foreach(string s in columnHeadings)
            {
                tmp_s += s + ", ";
            }
            sw.WriteLine(tmp_s);

            int numColumns = dataColumnRow.GetLength(0);
            int numRows = dataColumnRow.GetLength(1);

            //Dump data to file
            for (int i = 0; i < numColumns; i++)
            {
                for (int j = 0; j < numRows; j++)
                {
                    sw.Write(dataColumnRow[i, j]);
                    if (i == numRows - 1)
                        sw.Write("\r\n");
                    else
                        sw.Write(", ");
                }
            }

            //Cleanup
            sw.Close();
            sw = null;
            fs = null;
        }
    }
}
