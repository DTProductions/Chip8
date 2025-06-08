using SDL2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Chip8Interpreter {
	internal class Interpreter {
		private byte[] memory = new byte[4096];
		private Stack<ushort> callStack = new Stack<ushort>();
		private byte[] registers = new byte[16];
		private ushort indexRegister;
		private int delayTimer;
		private int soundTimer;
		private System.Timers.Timer timer;
		private ushort pc;
		private SdlDisplay screen;
		private SdlKeypad keypad;
		private SdlAudioManager audioManager;
		private Random rand = new Random();

		private bool useOriginalShift;
		private bool useOriginalJumpWithOffset;
		private bool useOriginalMemoryHandling;

		public Interpreter(SdlDisplay screen, SdlKeypad keypad, string instructionsPath, SdlAudioManager audioManager, bool useOriginalShift, bool useOriginalJumpWithOffset, bool useOriginalMemoryHandling) {
			this.screen = screen;
			this.keypad = keypad;
			this.audioManager = audioManager;

			this.useOriginalShift = useOriginalShift;
			this.useOriginalJumpWithOffset = useOriginalJumpWithOffset;
			this.useOriginalMemoryHandling = useOriginalMemoryHandling;


			timer = new System.Timers.Timer(1000.0 / 60);
			timer.Elapsed += DecrementTimers;
			timer.AutoReset = true;
			timer.Start();

			LoadInstructions(instructionsPath);
			LoadFont();
		}

		public ushort FetchInstruction() {
			byte firstByte = memory[pc];
			byte secondByte = memory[pc + 1];

			return (ushort)((firstByte << 8 | secondByte));
		}

		public void ClearScreen() {
			screen.Clear();
			screen.Render();
		}

		public byte GetRegister(ushort index) {
			return registers[index];
		}

		public void SetRegister(ushort index, byte value) {
			registers[index] = value;
		}

		public void AddToRegister(ushort index, byte value) {
			registers[index] += value;
		}

		public void SubtractFromRegister(ushort index, byte value) {
			registers[index] -= value;
		}

		public void SetIndexRegister(ushort value) {
			indexRegister = value;
		}

		public void AddToIndexRegister(ushort value) {
			indexRegister += value;
		}

		public void SetSoundTimer(byte value) {
			soundTimer = value;
		}

		public void SetDelayTimer(byte value) {
			delayTimer = value;
		}

		public byte GetDelayTimer() {
			return (byte) delayTimer;
		}

		public void GoToNextInstruction() {
			pc += 2;
		}

		public void JumpIfKeyDown(byte keycode) {
			if (keypad.IsKeyDown(keycode)) {
				pc += 2;
			}
		}

		public void JumpIfKeyUp(byte keycode) {
			if (!keypad.IsKeyDown(keycode)) {
				pc += 2;
			}
		}

		public void JumpInstruction(ushort baseAddress, byte offset) {
			if (useOriginalJumpWithOffset) {
				pc = (ushort)(baseAddress + registers[0]);
			}
			else {
				pc = (ushort)(baseAddress + offset);
			}
		}

		public void WaitForKey(ushort registerIndex) {
			byte? waitedKey = keypad.WaitForKey();
			if (waitedKey == null) {
				pc -= 2;
				return;
			}

			registers[registerIndex] = (byte)waitedKey;
		}

		private void LoadInstructions(string path) {
			byte[] buffer = File.ReadAllBytes(path);

			for (int i = 0; i < buffer.Length; i++) {
				memory[512 + i] = buffer[i];
			}

			pc = 512;
		}

		public void LoadRegistersIntoMemory(byte topRegisterIndex) {
			if (useOriginalMemoryHandling) {
				for (int i = 0; i <= topRegisterIndex; i++, indexRegister++) {
					memory[indexRegister] = registers[i];
				}
			} else {
				for (int i = 0; i <= topRegisterIndex; i++) {
					memory[indexRegister + i] = registers[i];
				}
			}
		}

		public void LoadMemoryIntoRegisters(byte topRegisterIndex) {
			if (useOriginalMemoryHandling) {
				for (int i = 0; i <= topRegisterIndex; i++, indexRegister++) {
					registers[i] = memory[indexRegister];
				}
			} else {
				for (int i = 0; i <= topRegisterIndex; i++) {
					registers[i] = memory[indexRegister + i];
				}
			}
		}

		public void BinaryToDecimalInstruction(byte toConvert) {
			memory[indexRegister] = (byte) (toConvert / 100);
			memory[indexRegister + 1] = (byte) ((toConvert / 10) % 10);
			memory[indexRegister + 2] = (byte) (toConvert % 10);
		}

		public void LoadFontCharIntoIndexRegister(byte character) {
			indexRegister = memory[0x50 + registers[character] * 5];
		}

		public void GenerateRandomNumberInRegister(byte registerIndex, byte binaryComparator) {
			registers[registerIndex] = (byte) (rand.Next(256) & binaryComparator);
		}

		public void ShiftRegisterBitLeft(byte shiftedRegisterIndex, byte assignedRegisterIndex) {
			if (useOriginalShift) {
				registers[shiftedRegisterIndex] = registers[assignedRegisterIndex];
			}

			// used to ensure that the flag is only set after the bit was shifted
			// in case shiftedRegisterIndex is the flag register itself.
			byte valueBeforeShifting = registers[shiftedRegisterIndex];

			registers[shiftedRegisterIndex] <<= 1;
			registers[15] = (byte)((valueBeforeShifting >> 7) & 0x1);
		}

		public void ShiftRegisterBitRight(byte shiftedRegisterIndex, byte assignedRegisterIndex) {
			if (useOriginalShift) {
				registers[shiftedRegisterIndex] = registers[assignedRegisterIndex];
			}

			// used to ensure that the flag is only set after the bit was shifted
			// in case shiftedRegisterIndex is the flag register itself.
			byte valueBeforeShifting = registers[shiftedRegisterIndex];

			registers[shiftedRegisterIndex] >>= 1;
			registers[15] = (byte)(valueBeforeShifting & 0x1);
		}

		public void SkipInstructionIfEquals(byte a, byte b) {
			if(a == b) {
				pc += 2;
			}
		}

		public void SkipInstructionIfNotEquals(byte a, byte b) {
			if (a != b) {
				pc += 2;
			}
		}

		public void ReturnFromSubroutine() {
			pc = callStack.Pop();
		}

		public void CallSubroutine(ushort subroutineStart) {
			callStack.Push(pc);
			pc = subroutineStart;
		}

		public void SetSumOverflowFlag(byte value, byte addedValue) {
			if (value + addedValue > byte.MaxValue) {
				registers[15] = 1;
			}
			else {
				registers[15] = 0;
			}
		}

		public void SetSubtractionUnderflowFlag(byte minuend, byte subtrahend) {
			if((minuend >= subtrahend)) {
				registers[15] = 1;
			}
			else {
				registers[15] = 0;
			}
		}

		public void Jump(ushort location) {
			pc = location;
		}

		public void Draw(byte xCoordRegisterIndex, byte yCoordRegisterIndex, byte height) {
			byte xCoord = (byte) (registers[xCoordRegisterIndex] % screen.Width);
			byte yCoord = (byte) (registers[yCoordRegisterIndex] % screen.Height);

			registers[15] = 0;

			for (int row = 0; row < height; row++) {
				byte spriteRow = memory[indexRegister + row];

				for(int column = 0; column < 8; column++) {
					if (GetBit(spriteRow, column) && (xCoord + column < screen.Width) && (yCoord + row < screen.Height)) {

						// if pixel will be turned off, set flag register
						if(screen.GetPixel(xCoord + column, yCoord + row)) {
							registers[15] = 1;
						}

						bool flippedPixel = !screen.GetPixel(xCoord + column, yCoord + row);
						screen.SetPixel(xCoord + column, yCoord + row, flippedPixel);
					}
				}
			}

			screen.Render();
		}

		// From left to right
		public static bool GetBit(byte b, int bitNumber) {
			return (b & (128 >> bitNumber)) != 0;
		}

		private void LoadFont() {

			byte[] font = new byte[] {
				0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
				0x20, 0x60, 0x20, 0x20, 0x70, // 1
				0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
				0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
				0x90, 0x90, 0xF0, 0x10, 0x10, // 4
				0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
				0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
				0xF0, 0x10, 0x20, 0x40, 0x40, // 7
				0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
				0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
				0xF0, 0x90, 0xF0, 0x90, 0x90, // A
				0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
				0xF0, 0x80, 0x80, 0x80, 0xF0, // C
				0xE0, 0x90, 0x90, 0x90, 0xE0, // D
				0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
				0xF0, 0x80, 0xF0, 0x80, 0x80  // F
			};

			for(int i = 0; i < font.Length; i++) {
				memory[i + 0x50] = font[i];
			}
		}

		private void DecrementTimers(Object? source, ElapsedEventArgs e) {
			if(delayTimer > 0) {
				Interlocked.Decrement(ref delayTimer);
			}

			if(soundTimer > 0) {
				if (!audioManager.IsRunning()) {
					audioManager.Start();
				}

				Interlocked.Decrement(ref soundTimer);
			} else if (audioManager.IsRunning()) {
				audioManager.Stop();
			}
		}
	}
}
