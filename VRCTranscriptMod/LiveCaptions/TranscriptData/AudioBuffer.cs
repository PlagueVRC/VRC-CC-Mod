

using System.Threading;
using VRCCCMod.CC.GameSpecific;

#if USE_SHORT
using BUFFER_TYPE = System.Int16;
#else
using BUFFER_TYPE = System.Single;
#endif

namespace VRCCCMod.CC.TranscriptData {
    /// <summary>
    /// Audio buffer that contains additional useful utilities
    /// </summary>
    class AudioBuffer {
        public const int buffer_size = 16000;
        public BUFFER_TYPE[] buffer = new BUFFER_TYPE[buffer_size];
        public int buffer_head = 0;

        public Mutex readWriteMutex = new Mutex();

        public bool beingTranscribed = false;
        public bool queued = false;

        public float lastFillTime = 0.0f;

#if LOG_COUNTS
        public static int count = 0;
        public static object count_lock = new object();

        public AudioBuffer() {
            lock(count_lock) {
                count++;
                GameUtils.LogDebug("New buffer. Total count: " + count.ToString());
            }
        }

        ~AudioBuffer() {
            lock(count_lock) {
                count--;
                GameUtils.LogDebug("Destroy buffer. Total count: " + count.ToString());
            }
        }
#endif

        public void StartTranscribing() {
            readWriteMutex.WaitOne();
            beingTranscribed = true;
        }

        public void StopTranscribing() {
            readWriteMutex.ReleaseMutex();
            beingTranscribed = false;
            buffer_head = 0;

            queued = false;
        }

        public bool ShouldBeQueued() {
            return buffer_head > (buffer_size - 1000);
        }
    }
}
