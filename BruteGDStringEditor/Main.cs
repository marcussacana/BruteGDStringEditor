using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BruteGDStringEditor {
    public class GlobalDataStringEditor {
        private byte[] Script;

        private int Pos;
        private int EndPos;
        private int[] Offsets;
        public GlobalDataStringEditor(byte[] Script) {
            this.Script = Script;
        }

        
        public string[] Import() {
            DetectStrings();
            string[] Strings = new string[Offsets.Length + 1];
            for (int i = 0, p = Pos; i < Strings.Length; i++) {
                int len = 0;
                while (Script[p + len] != 0x00)
                    len++;
                byte[] Buffer = new byte[len];
                Array.Copy(Script, p, Buffer, 0, len);
                Strings[i] = Encoding.UTF8.GetString(Buffer);
                p += len + 1;
            }
            DetectOffsets();
            return Strings;
        }

        public byte[] Export(string[] Strs) {
            if (Strs.Length - 1 != Offsets.Length)
                throw new Exception("You can't add/del string entries");            
            byte[] FirstPart = new byte[Pos];
            Array.Copy(Script, FirstPart, FirstPart.Length);

            byte[] SecondPart = new byte[0];
            for (int i = 0; i < Strs.Length; i++) {
                bool IsLast = !(i + 1 < Strs.Length);
                byte[] Buffer = Encoding.UTF8.GetBytes(IsLast ? Strs[i] : Strs[i] + "\x0");
                if (i != 0) {
                    int Offset = SecondPart.Length;
                    GenDW(Offset).CopyTo(FirstPart, Offsets[i - 1]);
                }
                Array.Resize(ref SecondPart, SecondPart.Length + Buffer.Length);
                Array.Copy(Buffer, 0, SecondPart, SecondPart.Length - Buffer.Length, Buffer.Length);
            }

            byte[] ThirdPart = new byte[Script.Length - (EndPos + 1)];
            Array.Copy(Script, EndPos + 1, ThirdPart, 0, ThirdPart.Length);

            byte[] Result = FirstPart.Concat(SecondPart).ToArray().Concat(ThirdPart).ToArray();

            UpdateCodeStart(ref Result);
            int Diff = Result.Length - Script.Length;
            GenDW(GetDW(0x20) + Diff).CopyTo(Result, 0x20);
            GenDW(GetDW(0x2C) + Diff).CopyTo(Result, 0x2C);

            return Result;
        }

        private void UpdateCodeStart(ref byte[] File) {
            byte[] Match = Encoding.ASCII.GetBytes("CODE_START");
            for (int i = 0; i < File.Length; i++) {
                if (EqualsAt(Match, File, i)) {
                    if (i % 16 != 0) {
                        int Del = i % 16;
                        int Add = 16 - Del;
                        if (Add <= Del) {
                            byte[] First = new byte[i + Add];
                            Array.Copy(File, 0, First, 0, i);

                            byte[] Second = new byte[File.Length - i];
                            Array.Copy(File, i, Second, 0, Second.Length);

                            File = First.Concat(Second).ToArray();
                            return;
                        } else {

                            byte[] First = new byte[i - Del];
                            Array.Copy(File, 0, First, 0, i - Del);

                            byte[] Second = new byte[File.Length - i];
                            Array.Copy(File, i, Second, 0, Second.Length);

                            File = First.Concat(Second).ToArray();
                            return;
                        }
                    }                        
                }
            }
        }

        private void DetectOffsets() {
            for (int p = Pos - 0x17C, i = Offsets.Length - 1; i > -1; i--) {
                byte[] DW = GenDW(Offsets[i]);
                while (p > 0 && !EqualsAt(DW, Script, --p))
                    continue;
                if (p <= 0)
                    throw new Exception("Failed to Search File Offsets");
                Offsets[i] = p;
            }
        }

        private byte[] GenDW(int val) => BitConverter.GetBytes(val);

        private void DetectStrings() {
            byte[] Sig = Encoding.ASCII.GetBytes("GBNL");
            int SigPos = 0x40;
            for (; SigPos < Script.Length; SigPos++)
                if (EqualsAt(Sig, Script, SigPos))
                    break;
            if (SigPos >= Script.Length)
                throw new Exception("BruteForce Unsupported");

            Pos = SigPos;
            while (Script[--Pos] == 0x00)
                continue;
            EndPos = Pos;
            SigPos -= 2;
            while ((GetDW(--Pos) & 0x00FFFFFF) != 0x00)
                continue;
            while (Script[++Pos] == 0x00)
                continue;
            
            List<int> Offs = new List<int>();
            for (int i = Pos; i < EndPos; i++) {
                if (Script[i] == 0x00) {
                    int Offset = (i + 1) - Pos;
                    Offs.Add(Offset);
                }
            }
            Offsets = Offs.ToArray();
        }

        private bool EqualsAt(byte[] DataToCompare, byte[] Data, int Pos) {
            if (DataToCompare.Length + Pos > Data.Length)
                return false;
            for (int i = 0; i < DataToCompare.Length; i++)
                if (DataToCompare[i] != Data[i + Pos])
                    return false;
            return true;
        }
        private int GetDW(int Pos) => BitConverter.ToInt32(Script, Pos);
    }
}
