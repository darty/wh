using System;
using System.Net;
using System.Windows;

namespace Pitch
{
    public class Yin
    {
        /**
         *
         * Code converted from Java to C# by Ville Valta
         * 
         * Java source:https://github.com/JorenSix/TarsosDSP/blob/master/src/be/hogent/tarsos/dsp/pitch/Yin.java
         * 
         * https://github.com/JorenSix/TarsosDSP
         * Based on Yin pitch tracking algorithms Java implementation by Joren Six
         * originally based on http://aubio.org 
         * See http://recherche.ircam.fr/equipes/pcm/cheveign/ps/2002_JASA_YIN_proof.pdf
         */
        private static readonly double DEFAULT_THRESHOLD = 0.20;

	/**
	 * The default size of an audio buffer (in samples).
	 */
	public static readonly int DEFAULT_BUFFER_SIZE = 2048;

	/**
	 * The default overlap of two consecutive audio buffers (in samples).
	 */
	public static readonly int DEFAULT_OVERLAP = 1536;

	/**
	 * The actual YIN threshold.
	 */
	private readonly double threshold;

	/**
	 * The audio sample rate. Most audio has a sample rate of 44.1kHz.
	 */
	private readonly float sampleRate;

	/**
	 * The buffer that stores the calculated values. It is exactly half the size
	 * of the input buffer.
	 */
	private readonly float[] yinBuffer;

	/**
	 * The result of the pitch detection iteration.
	 */
	private readonly PitchDetectionResult result;

	/**
	 * Create a new pitch detector for a stream with the defined sample rate.
	 * Processes the audio in blocks of the defined size.
	 * 
	 * @param audioSampleRate
	 *            The sample rate of the audio stream. E.g. 44.1 kHz.
	 * @param bufferSize
	 *            The size of a buffer. E.g. 1024.
	 */
	public Yin(float audioSampleRate,  int bufferSize) {
        this.sampleRate = audioSampleRate;
        this.threshold = DEFAULT_THRESHOLD;
        yinBuffer = new float[bufferSize / 2];
        result = new PitchDetectionResult();
	}

	/**
	 * Create a new pitch detector for a stream with the defined sample rate.
	 * Processes the audio in blocks of the defined size.
	 * 
	 * @param audioSampleRate
	 *            The sample rate of the audio stream. E.g. 44.1 kHz.
	 * @param bufferSize
	 *            The size of a buffer. E.g. 1024.
	 * @param yinThreshold
	 *            The parameter that defines which peaks are kept as possible
	 *            pitch candidates. See the YIN paper for more details.
	 */
	public Yin( float audioSampleRate,  int bufferSize,  double yinThreshold) {
		this.sampleRate = audioSampleRate;
		this.threshold = yinThreshold;
		yinBuffer = new float[bufferSize / 2];
		result = new PitchDetectionResult();
	}

	/**
	 * The main flow of the YIN algorithm. Returns a pitch value in Hz or -1 if
	 * no pitch is detected.
	 * 
	 * @return a pitch value in Hz or -1 if no pitch is detected.
	 */
	public PitchDetectionResult getPitch(float[] audioBuffer) {

		 int tauEstimate;
		 float pitchInHertz;

		// step 2
		difference(audioBuffer);

		// step 3
		cumulativeMeanNormalizedDifference();

		// step 4
		tauEstimate = absoluteThreshold();

		// step 5
		if (tauEstimate != -1) {
			 float betterTau = parabolicInterpolation(tauEstimate);

			// step 6
			// TODO Implement optimization for the AUBIO_YIN algorithm.
			// 0.77% => 0.5% error rate,
			// using the data of the YIN paper
			// bestLocalEstimate()

			// conversion to Hz
			pitchInHertz = sampleRate / betterTau;
		} else{
			// no pitch found
			pitchInHertz = -1;
		}

		result.setPitch(pitchInHertz);

		return result;
	}

	/**
	 * Implements the difference function as described in step 2 of the YIN
	 * paper.
	 */
	private void difference( float[] audioBuffer) {
		int index, tau;
		float delta;
		for (tau = 0; tau < yinBuffer.Length; tau++) {
			yinBuffer[tau] = 0;
		}
        for (tau = 1; tau < yinBuffer.Length; tau++)
        {
            for (index = 0; index < yinBuffer.Length; index++)
            {
				delta = audioBuffer[index] - audioBuffer[index + tau];
                //System.Diagnostics.Debug.WriteLine("---------------------------------");
                yinBuffer[tau] += delta * delta;
                //System.Diagnostics.Debug.WriteLine(yinBuffer[tau]);
			}
		}
	}

	/**
	 * The cumulative mean normalized difference function as described in step 3
	 * of the YIN paper. <br>
	 * <code>
	 * yinBuffer[0] == yinBuffer[1] = 1
	 * </code>
	 */
	private void cumulativeMeanNormalizedDifference() {
		int tau;
		yinBuffer[0] = 1;
		float runningSum = 0;
        for (tau = 1; tau < yinBuffer.Length; tau++)
        {
			runningSum += yinBuffer[tau];
			yinBuffer[tau] *= tau / runningSum;
		}
	}

	/**
	 * Implements step 4 of the AUBIO_YIN paper.
	 */
	private int absoluteThreshold() {
		// Uses another loop construct
		// than the AUBIO implementation
		int tau;
		// first two positions in yinBuffer are always 1
		// So start at the third (index 2)
        for (tau = 2; tau < yinBuffer.Length; tau++)
        {
			if (yinBuffer[tau] < threshold) {
                while (tau + 1 < yinBuffer.Length && yinBuffer[tau + 1] < yinBuffer[tau])
                {
					tau++;
				}
				// found tau, exit loop and return
				// store the probability
				// From the YIN paper: The threshold determines the list of
				// candidates admitted to the set, and can be interpreted as the
				// proportion of aperiodic power tolerated
				// within a periodic signal.
				//
				// Since we want the periodicity and and not aperiodicity:
				// periodicity = 1 - aperiodicity
				result.setProbability(1 - yinBuffer[tau]);
				break;
			}
		}


		// if no pitch found, tau => -1
        if (tau == yinBuffer.Length || yinBuffer[tau] >= threshold)
        {
			tau = -1;
			result.setProbability(0);
			result.setPitched(false);	
		} else {
			result.setPitched(true);
		}

		return tau;
	}

	/**
	 * Implements step 5 of the AUBIO_YIN paper. It refines the estimated tau
	 * value using parabolic interpolation. This is needed to detect higher
	 * frequencies more precisely. See http://fizyka.umk.pl/nrbook/c10-2.pdf and
	 * for more background
	 * http://fedc.wiwi.hu-berlin.de/xplore/tutorials/xegbohtmlnode62.html
	 * 
	 * @param tauEstimate
	 *            The estimated tau value.
	 * @return A better, more precise tau value.
	 */
	private float parabolicInterpolation( int tauEstimate) {
		 float betterTau;
		 int x0;
		 int x2;

		if (tauEstimate < 1) {
			x0 = tauEstimate;
		} else {
			x0 = tauEstimate - 1;
		}
        if (tauEstimate + 1 < yinBuffer.Length)
        {
			x2 = tauEstimate + 1;
		} else {
			x2 = tauEstimate;
		}
		if (x0 == tauEstimate) {
			if (yinBuffer[tauEstimate] <= yinBuffer[x2]) {
				betterTau = tauEstimate;
			} else {
				betterTau = x2;
			}
		} else if (x2 == tauEstimate) {
			if (yinBuffer[tauEstimate] <= yinBuffer[x0]) {
				betterTau = tauEstimate;
			} else {
				betterTau = x0;
			}
		} else {
			float s0, s1, s2;
			s0 = yinBuffer[x0];
			s1 = yinBuffer[tauEstimate];
			s2 = yinBuffer[x2];
			// fixed AUBIO implementation, thanks to Karl Helgason:
			// (2.0f * s1 - s2 - s0) was incorrectly multiplied with -1
			betterTau = tauEstimate + (s2 - s0) / (2 * (2 * s1 - s2 - s0));
		}
		return betterTau;
	}
    }
}
