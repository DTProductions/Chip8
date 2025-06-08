using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Interpreter {
	internal class SdlKeypad {

		private bool[] pressedKeys = new bool[16];
		private bool closeButtonPressed = false;
		private bool waitingKeyPress = false;
		private byte? waitedKeyPress = null;

		public bool IsKeyDown(byte key) {
			return pressedKeys[key];
		}

		public byte? WaitForKey() {
			waitingKeyPress = true;

			byte? keycode = waitedKeyPress;

			if (waitedKeyPress != null) {
				waitedKeyPress = null;
				waitingKeyPress = false;
			}

			return keycode;
		}

		private byte? MapScancodeToKey(SDL.SDL_Scancode scancode) {
			switch(scancode) {
				case SDL.SDL_Scancode.SDL_SCANCODE_1:
					return 0x1;
				case SDL.SDL_Scancode.SDL_SCANCODE_2:
					return 0x2;
				case SDL.SDL_Scancode.SDL_SCANCODE_3:
					return 0x3;
				case SDL.SDL_Scancode.SDL_SCANCODE_4:
					return 0xC;
				case SDL.SDL_Scancode.SDL_SCANCODE_Q:
					return 0x4;
				case SDL.SDL_Scancode.SDL_SCANCODE_W:
					return 0x5;
				case SDL.SDL_Scancode.SDL_SCANCODE_E:
					return 0x6;
				case SDL.SDL_Scancode.SDL_SCANCODE_R:
					return 0xD;
				case SDL.SDL_Scancode.SDL_SCANCODE_A:
					return 0x7;
				case SDL.SDL_Scancode.SDL_SCANCODE_S:
					return 0x8;
				case SDL.SDL_Scancode.SDL_SCANCODE_D:
					return 0x9;
				case SDL.SDL_Scancode.SDL_SCANCODE_F:
					return 0xE;
				case SDL.SDL_Scancode.SDL_SCANCODE_Z:
					return 0xA;
				case SDL.SDL_Scancode.SDL_SCANCODE_X:
					return 0x0;
				case SDL.SDL_Scancode.SDL_SCANCODE_C:
					return 0xB;
				case SDL.SDL_Scancode.SDL_SCANCODE_V:
					return 0xF;
				default:
					return null;
			}
		}

		public bool ClosedButtonPressed() {
			return closeButtonPressed;
		}

		public void HandleInput() {
			while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1) {
				switch (e.type) {
					case SDL.SDL_EventType.SDL_QUIT:
						closeButtonPressed = true;
						break;
					case SDL.SDL_EventType.SDL_KEYDOWN: {
						byte? keycode = MapScancodeToKey(e.key.keysym.scancode);
						if (keycode == null) {
							break;
						}

						pressedKeys[(int) keycode] = true;
						break;
					}
					case SDL.SDL_EventType.SDL_KEYUP: {
						byte? keycode = MapScancodeToKey(e.key.keysym.scancode);
						if (keycode == null) {
							break;
						}

						if (waitingKeyPress) {
							waitedKeyPress = keycode;
						}

						pressedKeys[(int) keycode] = false;
						break;
					}
				}
			}
		}
	}
}
