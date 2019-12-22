using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace _7DTD_SingleMapRenderer.Util
{
    public class SignatureScanner
    {
        //private string[] gamesymbols = new string[]
        //{
        //    "ui_game_symbol_map_cabin",
        //    "ui_game_symbol_map_campsite",
        //    "ui_game_symbol_map_cave",
        //    "ui_game_symbol_map_city",
        //    "ui_game_symbol_map_civil",
        //    "ui_game_symbol_map_fortress",
        //    "ui_game_symbol_map_house",
        //    "ui_game_symbol_map_town",
        //    "ui_game_symbol_map_trader",
        //    "ui_game_symbol_x",
        //    "ui_game_symbol_bicycle",
        //    "ui_game_symbol_minibike",
        //    "ui_game_symbol_motorcycle",
        //    "ui_game_symbol_4x4"
        //};

        //private List<byte[]> symbolsToFind;
        byte[] firstSymbol;

        public SignatureScanner()
        {
            //symbolsToFind = new List<byte[]>();
            //foreach (var symbol in gamesymbols)
            //{
            //    symbolsToFind.Add(Encoding.ASCII.GetBytes(symbol));
            //}

            firstSymbol = Encoding.ASCII.GetBytes("ui_game_symbol");
        }

        public int Find(byte[] array)
        {
            int matchingPos = 0;
            try
            {
                int i = 0;
                int j = 0;
                int length = array.Length;
                int length2 = firstSymbol.Length;
                while (i < length && j < length2)
                {
                    bool isMatch = array[i] == firstSymbol[j];
                    if (isMatch)
                    {
                        if (j == 0)
                            matchingPos = i;
                        j++;
                    }
                    else
                    {
                        j = 0;
                        if (matchingPos != 0)
                        {
                            i = matchingPos; // reset to first matching pos, for "partial matches"
                            matchingPos = 0;
                        }
                    }
                    bool isComplete = (j >= length2);
                    if (isComplete)
                    {
                        return matchingPos; // The signature was found, return it.
                    }

                    i++;
                }
                return 0;  // The signature was not found.
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
