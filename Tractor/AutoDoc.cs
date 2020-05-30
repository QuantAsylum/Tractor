﻿using Com.QuantAsylum.Tractor.Tests;
using Com.QuantAsylum.Tractor.Tests.GainTests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Tractor
{
    static class AutoDoc
    {
        static List<TestBase> Classes = new List<TestBase>();

        static Dictionary<string, string> Dic = new Dictionary<string, string>();

        static public void Dump()
        {
            StringBuilder sb = new StringBuilder();

            BuildDictionary();

            BuildPreamble(sb);

            FindDerivedClasses(sb);

            File.WriteAllText(Constants.AutoDocFile, sb.ToString());
        }

        static void FindDerivedClasses(StringBuilder sb)
        {
            var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.IsSubclassOf(typeof(TestBase))
                                     && t.GetConstructor(Type.EmptyTypes) != null
                            select Activator.CreateInstance(t) as TestBase;

            Classes = instances.Cast<TestBase>().ToList();

            Classes = Classes.OrderBy(o => o.Name).ToList();
            
            foreach (TestBase tb in Classes)
            {
                if (tb.Name == "Placeholder")
                    continue;

                if (tb.Name == "GainSorted3A01N")
                {

                }

                sb.AppendLine($"## {tb.Name}");
                sb.AppendLine($"### Test Category: {tb.TestType}");

                sb.AppendLine(tb.GetTestDescription() + Environment.NewLine + Environment.NewLine);

                FieldInfo[] f = tb.GetType().GetFields();
                f = f.OrderBy(m => m.GetCustomAttribute<ObjectEditorAttribute>().Index).ToArray();

                foreach (FieldInfo fi in f)
                {
                    if (fi.FieldType == typeof(ObjectEditorSpacer))
                        continue;

                    List<ObjectEditorAttribute> attribs = fi.GetCustomAttributes().Cast<ObjectEditorAttribute>().ToList();
                    attribs = attribs.OrderBy(o => o.Index).ToList();

                    foreach (ObjectEditorAttribute attr in attribs)
                    {
                        Dic.TryGetValue(attr.DisplayText, out string desc);
                        sb.AppendLine($"**{attr.DisplayText}:**  {desc ?? "???"}" + Environment.NewLine + Environment.NewLine);
                    }
                }
            }
        }

        static void BuildPreamble(StringBuilder sb)
        {
            sb.AppendLine("## Notice!");
            sb.AppendLine("This file is autogenerated. Please do not edit this file. This file can be recreated by running the Tractor application with the -H command line option. When -H is specified, the Tractor application " +
                "will run silently, and emit a markdown file named ```Tractor_Help.md``` in the Tractor data directory. Remove the -H command line argument to " +
                "run Tractor normally.");
        }

        static void BuildDictionary()
        {
            Dic.Add("Test Name", "The name of the test. This must be unique and should be something meaningful to you as you will see this name reflected in the logs");
            Dic.Add("Test Frequency (Hz)", "The frequency of the QA401 generator.");
            Dic.Add("Analyzer Output Level (dBV)", "The level of the QA401 generator.");
            Dic.Add("FFT Size (k)", "The size of the FFT that will be used for the test. Smaller FFTs run faster, while larger FFTs have more resolution");
            Dic.Add("Retry Count", "The number of times a failed test will be automatically retried before determining the failure is legitimate.");
            Dic.Add("Pre-analyzer Input Gain (dB)", "The amount of gain, in dB that is present at the input of the analyzer. Some instruments, such as the QA450, have built in " +
                "gain in their outputs. If so, that can be specified here. In the case of the QA450, it has 6 dB of built-in output attenuation. Thus, you'd enter a gain of -6 dB.");
            Dic.Add("Measure Left Channel", "If checked, the left channel will be displayed and measured. If not checked, the left channel will be muted and will not be displayed");
            Dic.Add("Measure Right Channel", "If checked, the right channel will be displayed and measured. If not checked, the right channel will be muted and will not be displayed");
            Dic.Add("Display Y Min", "The minimum value to be set for the Y axis. The HTML log will collect screen shots, and setting this ensures the screen capture is showing relevant regions.");
            Dic.Add("Display Y Max", "The maximum value to be set for the Y axis. The HTML log will collect screen shots, and setting this ensures the screen capture is showing relevant regions.");
            Dic.Add("Analyzer Input Range", "This specifies the maximum input level that will be used for the measurement. On the QA401, valid values are 6 and 26 dBV. Make sure to set this value correction to ensure you are getting the resolution you need.");

            Dic.Add("Load Impedance (ohms)", "Specifies the load impedance seen by the DUT.");
            Dic.Add("Arguments", "This specifies the arguments that will be passed to an executable specified in the Process Name.");
            Dic.Add("Process Name", "The name of the executable process to run.");
            Dic.Add("WAV File Name", "The name of the WAV file to be played. The WAV file must meet the requirements for sample rate (48K) and bit depth (24-bit integer or 32-bit float.");
            Dic.Add("Output Level (0..1)", "A value between 0 and 1 (inclusive) that will set the level of the WAV playback. A value of 1.0 means the WAV playback volume will be " +
                "unaltered. A value of 0.5 means the WAV playback volume will be half as loud (-6 dB) as present in the WAV file. ");

            Dic.Add("Mask File Name", "A mask file is created in the QA401 analyzer application. This is most often done by running the Test Plugin AmpFreqresponseChirp. If " +
                "you run that plug-in with at least 3 different DUTs (generating 3 different traces), and place all three of those measurements on the same graph, then you can use Math->Add or Edit Mask from " +
                "the Graph application. That will place the mask on the graph, and you can then use the Traces->Export Mask to export the mask.");

            Dic.Add("Amp Voltage", "The supply voltage of the amplifier. The current will be measured, and it's assumed the applied voltage is 'stiff' and won't sag during operation. The power input " +
                "will be calculated by the product of measured input current times the applied voltage specified here.");

            Dic.Add("Minimum Efficiency to Pass (%)", "The minimum efficiency required in order to pass the test. This is specifed as a number between 0 and 100.");
            Dic.Add("Maximum Efficiency to Pass (%)", "The maxium efficiency required in order to pass the test. This is specifed as a number between 0 and 100.");

            Dic.Add("Smoothing (1/N), N=", "The amount of smoothing to be applied to the frequency response sweep. Note that normally smoothing is defined as a fraction of an octave. " +
                "For example, if smooth over 1/48th of an octave, the smoothing is modest compared to smooth over 1/3rd of an octave. Here, you specify the numerator. So if you wished " +
                "for 1/3rd octave smoothing, you'd specify the value 3.");

            Dic.Add("Minimum Gain to Pass (dB)", "The minimum gain, in dB, required in order to pass the test.");
            Dic.Add("Maximum Gain to Pass (dB)", "The maximum gain, in dB, required in order to pass the test.");

            Dic.Add("Minimum THD to Pass (dB)", "The minimum THD, in dB, required in order to pass the test.");
            Dic.Add("Maximum THD to Pass (dB)", "The maximum THD, in dB, required in order to pass the test.");

            Dic.Add("Minimum THD+N to Pass (dB)", "The minimum THD+N, in dB, required in order to pass the test.");
            Dic.Add("Maximum THD+N to Pass (dB)", "The maximum THD+n, in dB, required in order to pass the test.");

            Dic.Add("Minimum Level to Pass (dBV)", "The minimum level, in dBV, required in order to pass the test.");
            Dic.Add("Maximum Level to Pass (dBV)", "The maximum level, in dBV, required in order to pass the test.");

            Dic.Add("Minimum Impedance to Pass (Ω)", "The minimum impedance, in ohms, required in order to pass the test.");
            Dic.Add("Maximum Impedance to Pass (Ω)", "The maximum impedance, in ohms, required in order to pass the test.");

            Dic.Add("Minimum Level to Pass (dBc)", "The minimum level, in dBc, required in order to pass the test.");
            Dic.Add("Maximum Level to Pass (dBc)", "The maximum level, in dBc, required in order to pass the test.");

            Dic.Add("Minimum Gain to Pass Sort 0 (dB)", "The minimum gain, in dB, required in order to pass the test while being 'bucketed' into bucket 0.");
            Dic.Add("Maximum Gain to Pass Sort 0 (dB)", "The maximum gain, in dB, required in order to pass the test while being 'bucketed' into bucket 0.");

            Dic.Add("Minimum Gain to Pass Sort 1 (dB)", "The minimum gain, in dB, required in order to pass the test while being 'bucketed' into bucket 1.");
            Dic.Add("Maximum Gain to Pass Sort 1 (dB)", "The maximum gain, in dB, required in order to pass the test while being 'bucketed' into bucket 1.");

            Dic.Add("Minimum Gain to Pass Sort 2 (dB)", "The minimum gain, in dB, required in order to pass the test while being 'bucketed' into bucket 2.");
            Dic.Add("Maximum Gain to Pass Sort 2 (dB)", "The maximum gain, in dB, required in order to pass the test while being 'bucketed' into bucket 2.");

            Dic.Add("Minimum Gain to Pass Sort 3 (dB)", "The minimum gain, in dB, required in order to pass the test while being 'bucketed' into bucket 3.");
            Dic.Add("Maximum Gain to Pass Sort 3 (dB)", "The maximum gain, in dB, required in order to pass the test while being 'bucketed' into bucket 3.");

            Dic.Add("Minimum Gain to Pass Sort 4 (dB)", "The minimum gain, in dB, required in order to pass the test while being 'bucketed' into bucket 4.");
            Dic.Add("Maximum Gain to Pass Sort 4 (dB)", "The maximum gain, in dB, required in order to pass the test while being 'bucketed' into bucket 4.");

            Dic.Add("Operator Instruction", "This is text that will appear when the operator sees this test screen. It can be used to give the operator an " +
                "instruction or to ask them a pass/fail test.");

            Dic.Add("Power Enabled", "Indicates if power is to turned on or off to the DUT. This is achieved via the solid state relay in the QA450. Note that " +
                "there are hardware-dependent limitations on swapping DUTs when the power is active. This is because connecting a new DUT with power active" +
                "can couse thousands of amps to flow as the output capacitors in the power supply are asked to charge the input capacitors in the DUT. Read " +
                "the manual to better understand the issues involved.");

            Dic.Add("Minimum Current to Pass (A)", "The minimum current, in amps, required in order to pass the test.");
            Dic.Add("Maximum Current to Pass (A)", "The maximum current, in amps, required in order to pass the test.");

            Dic.Add("RMS Measurement Start (Hz)", "The starting frequency of the indicated RMS measurement. ");
            Dic.Add("RMS Measurement Stop (Hz)", "The ending frequency of the indicated RMS measurement.");

            Dic.Add("Operator Message Sort 0", "The message displayed to the operator when the DUT resolves into sort bin 0. This test is useful if you are " +
                "sorting produced components for gain.");
            Dic.Add("Operator Message Sort 1", "The message displayed to the operator when the DUT resolves into sort bin 1. This test is useful if you are " +
                "sorting produced components for gain.");
            Dic.Add("Operator Message Sort 2", "The message displayed to the operator when the DUT resolves into sort bin 2. This test is useful if you are " +
                "sorting produced components for gain.");
            Dic.Add("Operator Message Sort 3", "The message displayed to the operator when the DUT resolves into sort bin 3. This test is useful if you are " +
                "sorting produced components for gain.");
            Dic.Add("Operator Message Sort 4", "The message displayed to the operator when the DUT resolves into sort bin 4. This test is useful if you are " +
                "sorting produced components for gain.");

            Dic.Add("Windowing (mS) (0=none)", "The windowing that will be applied to the recovered chirped impulse response. The indicated window specifies the " +
                "time time after the peak of the impulse response. A 1 mS gate period is always applied prior to the peak. In general, if you are working " +
                "with a microphone in a room that hasn't been specially treated for reflections, then you want to roughly use 1 mS of windowing time for every " +
                "one foot of distance the test setup has from a reflecting surface, including floors and walls. So, if your test setup is 3 feet (~1m) off the ground " +
                "and you have 3 feet free on all sides of the test setup, then a good first order value to use for the Windowing value is 3 mS. This should give a response " +
                "good down to 1/3mS = 333 Hz. That is, information below 333 Hz will be increasingly uncertain but values above that level will be increasingly certain. " +
                "Similarly, if you can provide a DUT setup that exhibits 9 feet (3m) of clearance in all directions (floor, ceiling, walls) then your lower bound of confidence " +
                "will be 1/9mS = 111 Hz.");

            Dic.Add("Load Impedance", "The impedance, measured in ohms, of the load presented to the DUT.");

            Dic.Add("Check Phase", "If checked, the phase of signals will be compared and flagged they do not agree.");

            Dic.Add("Prompt Message", "A message displayed to the operator. This can be an instruction that must be followed or, if the Display Fail Button option is checked " +
                "asking the operator to make a pass/fail assessment.");
            Dic.Add("Bitmap File Name", "This is the name of a bitmap file that will be displayed to the operator. The bitmap should be a PNG image with a resolution of 512x384. The " +
                "image will be stretched to fit, maintaining specified aspect ratio.");
            Dic.Add("Display Fail Button", "If chedcked, the operator will see a Fail button in addition to the Pass button.");

            Dic.Add("Timeout (sec)", "This is the amount of time an external operation can take before being terminated.");

            Dic.Add("Run Minimized", "If checked, the CMD window will run minimized and generally won't be visible to the operator. If unchecked, the operator will see the CMD window and " +
                "execution progress.");

            Dic.Add("Start Frequency (Hz)", "The starting frequency of the noise measurement for THD+N measurements.");
            Dic.Add("Stop Frequency (Hz)", "The stopping frequency of the noise measurement for THD+N measurements.");

            Dic.Add("", "");
        }
    }
}