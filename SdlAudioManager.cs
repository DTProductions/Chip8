using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Interpreter {
	internal class SdlAudioManager {

		private int frequency;
		private sbyte volume;
		private static int sampleRate = 44100;
		private ushort bufferSize = 4096;
		private static float phase = 0;
		private bool isRunning = false;

		public SdlAudioManager(int frequency, sbyte volume) {
			this.frequency = frequency;
			this.volume = volume;

			SDL.SDL_AudioSpec spec = new SDL.SDL_AudioSpec() {
				format = SDL.AUDIO_S8,
				channels = 1,
				freq = sampleRate,
				samples = bufferSize,
				callback = GenerateSamples,
			};

			if (SDL.SDL_OpenAudio(ref spec, out _) < 0) {
				throw new InvalidOperationException(SDL.SDL_GetError());
			}
		}

		public void Stop() {
			isRunning = false;
			SDL.SDL_PauseAudio(1);
		}

		public void Start() {
			isRunning = true;
			SDL.SDL_PauseAudio(0);
		}

		public bool IsRunning() {
			return isRunning;
		}

		private void GenerateSamples(nint userdata, nint stream, int len) {
			unsafe {
				sbyte* buffer = (sbyte*) stream;

				float phaseIncrement = frequency / (float) sampleRate;

				// drawing a square wave
				for (int i = 0; i < len; i++) {

					buffer[i] = (phase < 0.5f) ? volume : (sbyte) -volume;
					phase += phaseIncrement;
					if (phase >= 1f) {
						// -1 is used to avoid incorrect values when
						// phase goes over 1.
						// For instance, if 1.33 is reached, it goes back to
						// 0.33 of the next cycle, instead of reseting to 0.
						phase -= 1f;
					}
				}
			}
		}
	}
}
