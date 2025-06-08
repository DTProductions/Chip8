using SDL2;
using System;
using System.Timers;


namespace Chip8Interpreter {
	internal class Program {

		static void Main(string[] args) {
			if(args.Length < 1) {
				return;
			}

			string instructionsPath = args[0];

			bool useOriginalJumpWithOffset = true;
			bool useOriginalShift = true;
			bool useOriginalMemoryHandling = true;

			for(int i = 1; i < args.Length; i++) {
				switch (args[i]) {
					case "alt-jump":
						useOriginalJumpWithOffset = false;
						break;
					case "alt-shift":
						useOriginalShift = false;
						break;
					case "alt-mem-handl":
						useOriginalMemoryHandling = false;
						break;
				}
			}

			if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO) < 0) {
				throw new Exception(SDL.SDL_GetError());
			}

			SdlDisplay sdlDisplay = new SdlDisplay(64, 32, "Interpreter", 10);
			SdlKeypad keypad = new SdlKeypad();
			SdlAudioManager audioManager = new SdlAudioManager(220, 64);

			Interpreter interpreter = new Interpreter(sdlDisplay, keypad, instructionsPath, audioManager, useOriginalShift, useOriginalJumpWithOffset, useOriginalMemoryHandling);

			Decoder decoder = new Decoder(interpreter);

			while (!keypad.ClosedButtonPressed()) {
				keypad.HandleInput();
				decoder.ExecuteNextInstruction();
			}

			sdlDisplay.Dispose();
			SDL.SDL_Quit();
		}
	}
}

