using System;
using System.Reflection;
using SharpAudio;
using SharpAudio.Codec;

namespace RuutSpace
{
    class Program
        {
            private static string fileName;
            private static Scheduler sched = new Scheduler();
            
            static void Main(string[] args)
            {
                if (!sched.IsScheduled())
                {
                    sched.CreateSchedule();
                }
                
                fileName = GetSoundFileName();
                if (!fileName.Contains(Constants.FileEnding))
                {
                    Console.Error.WriteLine(Constants.FailedRessourceLoading);
                    return;
                }

                PlaySound();
            }

            private static void PlaySound()
            {
                var engine = AudioEngine.CreateDefault();
                var soundStream = new SoundStream(
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName),
                    engine);
                soundStream.Play();
            }

            private static string GetSoundFileName()
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resNames = assembly.GetManifestResourceNames();
                for (var i = 0; i < resNames.Length; ++i)
                {
                    if (resNames[i].Contains(Constants.FileEnding))
                    {
                        return resNames[i];
                    }
                }

                return "";
            }
        }
    }