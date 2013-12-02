﻿using System;
using System.Collections.Generic;
using System.IO;

namespace OSUplayer.OsuFiles
{
    public struct ScoreRecord
    {
        public double Acc;
        public int Hit100;
        public int Hit200;
        public int Hit300;
        public int Hit320;
        public int Hit50;
        public int MaxCombo;
        public int Miss;
        public string Mod;
        public modes Mode;
        public string Player;
        public int Score;
        public DateTime Time;
        public int Totalhit;
    }

    public class BinaryReader : System.IO.BinaryReader
    {
        public BinaryReader(Stream stream) : base(stream)
        {
        }

        public override string ReadString()
        {
            return ReadByte() == 0x0b ? base.ReadString() : "";
        }
    }

    internal static class OsuDB
    {
        private static int Scorecompare(ScoreRecord a, ScoreRecord b)
        {
            if (a.Score > b.Score)
            {
                return 1;
            }
            if (a.Score == b.Score)
            {
                return 0;
            }
            return -1;
        }

        public static void ReadScore(string file)
        {
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var reader = new BinaryReader(fs);
                reader.ReadInt32(); //version
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    string songmd5 = reader.ReadString();
                    int scorecount = reader.ReadInt32();
                    var Nscore = new List<ScoreRecord>();
                    for (int j = 0; j < scorecount; j++)
                    {
                        var Tscore = new ScoreRecord();
                        Tscore.Mode = (modes) reader.ReadByte();
                        reader.ReadInt32(); //version
                        reader.ReadString(); //set md5
                        Tscore.Player = reader.ReadString();
                        reader.ReadString(); //diff md5
                        Tscore.Hit300 = reader.ReadInt16();
                        Tscore.Hit100 = reader.ReadInt16();
                        Tscore.Hit50 = reader.ReadInt16();
                        Tscore.Hit320 = reader.ReadInt16();
                        Tscore.Hit200 = reader.ReadInt16();
                        Tscore.Miss = reader.ReadInt16();
                        Tscore.Score = reader.ReadInt32();
                        Tscore.MaxCombo = reader.ReadInt16();
                        reader.ReadBoolean(); //isperfect
                        Tscore.Mod = Modconverter(reader.ReadUInt32() + reader.ReadByte() << 32);
                        Tscore.Time = new DateTime(reader.ReadInt64());
                        reader.ReadInt32();
                        reader.ReadInt32();
                        Tscore.Acc = Getacc(Tscore);
                        Tscore.Totalhit = Tscore.Hit300 + Tscore.Hit100 + Tscore.Hit50 + Tscore.Miss;
                        Nscore.Add(Tscore);
                    }
                    Nscore.Sort(Scorecompare);
                    Core.Scores.Add(songmd5, Nscore);
                }
            }
        }

        public static void ReadDb(string file)
        {
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var reader = new BinaryReader(fs);
                reader.ReadInt32(); //version
                reader.ReadInt32(); //folders
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadByte();
                reader.ReadString(); //player
                string stashs;
                int stash;
                int mapcount = reader.ReadInt32();
                var tmpbm = new Beatmap();
                var tmpset = new BeatmapSet();
                for (int i = 0; i < mapcount; i++)
                {
                    tmpbm.ArtistRomanized = reader.ReadString();
                    stashs = reader.ReadString();
                    if (stashs != "")
                    {
                        tmpbm.Artist = stashs;
                    }
                    tmpbm.TitleRomanized = reader.ReadString();
                    stashs = reader.ReadString();
                    if (stashs != "")
                    {
                        tmpbm.Title = stashs;
                    }
                    tmpbm.Creator = reader.ReadString();
                    tmpbm.Version = reader.ReadString();
                    tmpbm.Audio = reader.ReadString();
                    tmpbm.Hash = reader.ReadString();
                    tmpbm.Name = reader.ReadString();
                    reader.ReadByte(); //4=ranked 5=app 2=Unranked
                    tmpbm.Totalhitcount = reader.ReadUInt16(); //circles
                    tmpbm.Totalhitcount += reader.ReadUInt16(); //sliders
                    tmpbm.Totalhitcount += reader.ReadUInt16(); //spinners
                    reader.ReadInt64(); //最后编辑
                    reader.ReadByte(); //AR
                    reader.ReadByte(); //CS
                    reader.ReadByte(); //HP
                    reader.ReadByte(); //OD
                    reader.ReadDouble(); //SV
                    reader.ReadInt32(); //playtime
                    reader.ReadInt32(); //totaltime
                    reader.ReadInt32(); //preview
                    stash = reader.ReadInt32(); //timting points 数
                    for (int j = 0; j < stash; j++)
                    {
                        reader.ReadDouble(); //bpm
                        reader.ReadDouble(); //offset
                        reader.ReadBoolean(); //红线
                    }
                    tmpbm.beatmapId = reader.ReadInt32();
                    tmpbm.beatmapsetId = reader.ReadInt32();
                    reader.ReadInt32(); //threadid 
                    reader.ReadByte(); //Ranking osu
                    reader.ReadByte(); //Ranking taiko
                    reader.ReadByte(); //Ranking ctb
                    reader.ReadByte(); //Ranking mania
                    tmpbm.Offset = reader.ReadInt16();
                    reader.ReadSingle(); //stack
                    tmpbm.mode = reader.ReadByte();
                    tmpbm.Source = reader.ReadString();
                    tmpbm.tags = reader.ReadString();
                    stash = reader.ReadInt16(); //online-offset
                    if (tmpbm.Offset == 0 && stash != 0)
                    {
                        tmpbm.Offset = stash;
                    }
                    reader.ReadString(); //title-font
                    reader.ReadBoolean(); //unplayed
                    reader.ReadInt64(); //last_play
                    reader.ReadBoolean(); //osz2
                    tmpbm.Location = Path.Combine(Core.osupath, reader.ReadString());
                    reader.ReadInt64(); //最后同步
                    reader.ReadBoolean(); //忽略音效
                    reader.ReadBoolean(); //忽略皮肤
                    reader.ReadBoolean(); //禁用sb
                    reader.ReadByte(); //背景淡化
                    reader.ReadInt64();
                    if (tmpset.count == 0)
                    {
                        tmpset.add(tmpbm);
                    }
                    else
                    {
                        if (tmpset.Contains(tmpbm))
                        {
                            tmpset.add(tmpbm);
                        }
                        else
                        {
                            Core.allsets.Add(tmpset);
                            tmpset = new BeatmapSet();
                            tmpset.add(tmpbm);
                        }
                    }
                    tmpbm = new Beatmap();
                }
            }
        }

        public static void ReadCollect(string file)
        {
            Core.Collections.Clear();
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var reader = new BinaryReader(fs);
                reader.ReadInt32(); //version
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    string title = reader.ReadString();
                    int itemcount = reader.ReadInt32();
                    var Nset = new List<int>();
                    for (int j = 0; j < itemcount; j++)
                    {
                        string md5 = reader.ReadString();
                        for (int k = 0; k < Core.allsets.Count; k++)
                        {
                            if (Core.allsets[k].md5.Contains(md5) && (!Nset.Contains(k)))
                            {
                                Nset.Add(k);
                            }
                        }
                    }
                    Core.Collections.Add(title, Nset);
                }
            }
        }
        private static double Getacc(ScoreRecord s)
        {
            switch (s.Mode)
            {
                case modes.Osu:
                    return (s.Hit300*6 + s.Hit100*2 + s.Hit50)
                           /(double) ((s.Hit300 + s.Hit100 + s.Hit50 + s.Miss)*6);
                case modes.Taiko:
                    return (s.Hit300*2 + s.Hit100)
                           /(double) ((s.Hit300 + s.Hit100 + s.Miss)*2);
                case modes.CTB:
                    return (s.Hit300 + s.Hit100 + s.Hit50)
                           /(double) (s.Hit300 + s.Hit100 + s.Hit50 + s.Hit200 + s.Miss);
                case modes.Mania:
                    return (s.Hit300*6 + s.Hit320*6 + s.Hit200*4 + s.Hit100*2 + s.Hit50)
                           /(double) ((s.Hit300 + s.Hit320 + s.Hit200 + s.Hit100 + s.Hit50 + s.Miss)*6);
                default:
                    return 0;
            }
        }
        private static string Modconverter(long mod)
        {
            string cmod = "";
            if (mod == 0)
            {
                cmod = "None";
            }
            else
            {
                for (int i = 0; i < (int) mods.Random; i++)
                {
                    if ((mod & 1) == 1)
                    {
                        cmod += " " + Enum.GetName(typeof (mods), i);
                    }
                    mod = mod >> 1;
                }
            }
            return cmod;
        }
    }
}