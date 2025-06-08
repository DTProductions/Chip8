using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Interpreter {
	internal class Decoder {
		private Interpreter interpreter;

		public Decoder(Interpreter interpreter) {
			this.interpreter = interpreter;
		}

		public void ExecuteNextInstruction() {

			ushort instruction = interpreter.FetchInstruction();
			interpreter.GoToNextInstruction();

			byte x = (byte)((instruction & 0xF00) >> 8);
			byte y = (byte)((instruction & 0xF0) >> 4);
			byte n = (byte)(instruction & 0xF);

			// second byte
			byte nn = (byte) (instruction & 0x00FF);
			ushort nnn = (ushort)(instruction & 0x0FFF);

			byte firstNibble = (byte)(instruction >> 12);
			switch (firstNibble) {
				case 0x0:
					if (instruction == 0x00E0) {
						interpreter.ClearScreen();
					}
					else if (instruction == 0x00EE) {
						interpreter.ReturnFromSubroutine();
					}
					break;
				case 0x1:
					interpreter.Jump(nnn);
					break;
				case 0x2:
					interpreter.CallSubroutine(nnn);
					break;
				case 0x3:
					interpreter.SkipInstructionIfEquals(interpreter.GetRegister(x), nn);
					break;
				case 0x4:
					interpreter.SkipInstructionIfNotEquals(interpreter.GetRegister(x), nn);
					break;
				case 0x5:
					interpreter.SkipInstructionIfEquals(interpreter.GetRegister(x), interpreter.GetRegister(y));
					break;
				case 0x6:
					interpreter.SetRegister(x, nn);
					break;
				case 0x7:
					interpreter.AddToRegister(x, nn);
					break;
				case 0x8:
					switch (n) {
						case 0x0:
							interpreter.SetRegister(x, interpreter.GetRegister(y));
							break;
						case 0x1:
							interpreter.SetRegister(x, (byte)(interpreter.GetRegister(x) | interpreter.GetRegister(y)));
							break;
						case 0x2:
							interpreter.SetRegister(x, (byte)(interpreter.GetRegister(x) & interpreter.GetRegister(y)));
							break;
						case 0x3:
							interpreter.SetRegister(x, (byte)(interpreter.GetRegister(x) ^ interpreter.GetRegister(y)));
							break;
						case 0x4: {
								byte originalValue = interpreter.GetRegister(x);
								byte addedValue = interpreter.GetRegister(y);

								interpreter.AddToRegister(x, interpreter.GetRegister(y));

								interpreter.SetSumOverflowFlag(originalValue, addedValue);
								break;
							}
						case 0x5: {
								byte minuend = interpreter.GetRegister(x);
								byte subtrahend = interpreter.GetRegister(y);

								interpreter.SubtractFromRegister(x, interpreter.GetRegister(y));

								interpreter.SetSubtractionUnderflowFlag(minuend, subtrahend);
								break;
							}
						case 0x6:
							interpreter.ShiftRegisterBitRight(x, y);
							break;
						case 0x7: {
								byte minuend = interpreter.GetRegister(y);
								byte subtrahend = interpreter.GetRegister(x);

								interpreter.SetRegister(x, (byte) (interpreter.GetRegister(y) - interpreter.GetRegister(x)));

								interpreter.SetSubtractionUnderflowFlag(minuend, subtrahend);
								break;
							}
						case 0xE:
							interpreter.ShiftRegisterBitLeft(x, y);
							break;
					}
					break;
				case 0x9:
					interpreter.SkipInstructionIfNotEquals(interpreter.GetRegister(x), interpreter.GetRegister(y));
					break;
				case 0xA:
					interpreter.SetIndexRegister(nnn);
					break;
				case 0xB:
					interpreter.JumpInstruction(nnn, interpreter.GetRegister(x));
					break;
				case 0xC:
					interpreter.GenerateRandomNumberInRegister(x, nn);
					break;
				case 0xD:
					interpreter.Draw(x, y, n);
					break;
				case 0xE:
					if (nn == 0x9E) {
						interpreter.JumpIfKeyDown(interpreter.GetRegister(x));
					}
					else if (nn == 0xA1) {
						interpreter.JumpIfKeyUp(interpreter.GetRegister(x));
					}
					break;
				case 0xF:
					switch (nn) {
						case 0x07:
							interpreter.SetRegister(x, interpreter.GetDelayTimer());
							break;
						case 0x15:
							interpreter.SetDelayTimer(interpreter.GetRegister(x));
							break;
						case 0x18:
							interpreter.SetSoundTimer(interpreter.GetRegister(x));
							break;
						case 0xA:
							interpreter.WaitForKey(x);
							break;
						case 0x1E:
							interpreter.AddToIndexRegister(interpreter.GetRegister(x));
							break;
						case 0x29:
							interpreter.LoadFontCharIntoIndexRegister(interpreter.GetRegister(x));
							break;
						case 0x33:
							interpreter.BinaryToDecimalInstruction(interpreter.GetRegister(x));
							break;
						case 0x55:
							interpreter.LoadRegistersIntoMemory(x);
							break;
						case 0x65:
							interpreter.LoadMemoryIntoRegisters(x);
							break;
					}
					break;
			}
		}
	}
}
