using System.Speech.Synthesis;

namespace VendingMachine
{
    class Audio
    {
        static bool inti = true;
        public static SpeechSynthesizer speechSynthesizerObj = new SpeechSynthesizer();
                
        public static void Speak(string msg)
        {
            try
            {
                if (inti)
                {
                    speechSynthesizerObj.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Teen);
                    speechSynthesizerObj.Rate = -2;
                }
                if (msg != null && msg.Length > 0)
                {
                    speechSynthesizerObj.SpeakAsyncCancelAll();
                    speechSynthesizerObj.SpeakAsync(msg);
                }

            }
            catch
            {

            }
            inti = false;
        }

    }
}
