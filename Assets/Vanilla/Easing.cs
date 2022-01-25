using System;

using UnityEngine;

namespace Vanilla.Easing
{

    /// <summary>
    ///     This is a C# implementation of Michealangelo007s incredible easing formula Javascript library.
    ///
    ///     That can be found at the following address:
    ///
    ///     https://github.com/Michaelangel007/easing
    ///
    ///     That library is itself based on Robert Penners infamous easing library from 2001, variations of
    ///     which can be found here:
    ///
    ///     http://robertpenner.com/easing/
    /// </summary>

    [Serializable]
    public struct EasingMethodSlot
    {

        [SerializeField]
        private Easing.EasingDirection _easingType;
        public Easing.EasingDirection easingType
        {
            get => _easingType;
            set
            {
                if (_easingType == value) return;

                _easingType = value;

                _method = GetCurrentMethod();
            }
        }

        [SerializeField]
        private Easing.EasingAlgorithmType _algorithm;
        public Easing.EasingAlgorithmType algorithm
        {
            get => _algorithm;
            set
            {
                if (_algorithm == value) return;

                _algorithm = value;

                _method = GetCurrentMethod();
            }
        }

        private Easing.EaseMethod _method;
        public  Easing.EaseMethod method => _method ??= GetCurrentMethod();


        public EasingMethodSlot
        (
            Easing.EasingDirection     easingDirection     = Easing.EasingDirection.InOut,
            Easing.EasingAlgorithmType easingAlgorithmType = Easing.EasingAlgorithmType.Quartic)
        {
            _easingType = easingDirection;
            _algorithm  = easingAlgorithmType;

            _method = Easing.GetEasingMethod(easingDirection: _easingType,
                                                    algorithmType: _algorithm);
        }


        private Easing.EaseMethod GetCurrentMethod() =>
            Easing.GetEasingMethod(easingDirection: _easingType,
                                   algorithmType: _algorithm);
        
        public void OnValidate() => _method = GetCurrentMethod();

    }

    public static class Easing
    {

        // -------------------------------------------------------------------------------------------------- Values //

        #region Constants



        const         double k      = 1.70158;
        const         double l0     = 7.5625;
        const         double r      = 1 / 2.75; // reciprocal
        const         double n      = 2.5949095;
        const float  halfPi = 1.57079637050629f;



        #endregion

        #region Enums



        /// <summary>
        ///     The different kinds of easing direction.
        /// </summary>
        public enum EasingDirection
        {

            In,
            Out,
            InOut

        }

        public enum EasingAlgorithmType
        {

            ConstantZero, // Returns a constant of 0
            ConstantOne,  // Returns a constant of 1
            Linear,       // Returns the input as-is
            Quadratic,    // Makes the output move towards/away from its target values, exaggerated to the power of 2
            Cubic,        // Makes the output move towards/away from its target values, exaggerated to the power of 3
            Quartic,      // Makes the output move towards/away from its target values, exaggerated to the power of 4
            Quintic,      // Makes the output move towards/away from its target values, exaggerated to the power of 5
            Sextic,       // Makes the output move towards/away from its target values, exaggerated to the power of 6
            Septic,       // Makes the output move towards/away from its target values, exaggerated to the power of 7
            Octic,        // Makes the output move towards/away from its target values, exaggerated to the power of 8
            Nonic,        // Makes the output move towards/away from its target values, exaggerated to the power of 9
            Decic,        // Makes the output move towards/away from its target values, exaggerated to the power of 10
            Back,         // Makes the output undershoot or overshoot its target values 
            Bounce,       // Makes the output bounce towards or away from its target values
            Circle,       // Makes the output follow a perfect circle
            Elastic       // Makes the output wobble towards or away from its target values

        }



        #endregion

        // ----------------------------------------------------------------------------------------- Delegate Easing //

        #region Delegate and delegate retrieval

        public delegate float EaseMethod(float i);


        /// <summary>
        ///     Returns a highly optimized float-returning method that corresponds with the given parameters.
        /// </summary>
        /// 
        /// <param name="easingDirection">
        ///     Does this easing happen on the way in, out, or both?
        /// </param>
        /// 
        /// <param name="algorithmType">
        ///     What kind of motion do we want applied? See the enum definition for details.
        /// </param>
        public static EaseMethod GetEasingMethodOld(EasingDirection     easingDirection,
                                                 EasingAlgorithmType algorithmType) =>
            easingDirection switch
            {
                EasingDirection.In => algorithmType switch
                                      {
                                          EasingAlgorithmType.ConstantZero => ConstantZero,
                                          EasingAlgorithmType.ConstantOne  => ConstantOne,
                                          EasingAlgorithmType.Linear       => Linear,
                                          EasingAlgorithmType.Quadratic    => InQuadratic,
                                          EasingAlgorithmType.Cubic        => InCubic,
                                          EasingAlgorithmType.Quartic      => InQuartic,
                                          EasingAlgorithmType.Quintic      => InQuintic,
                                          EasingAlgorithmType.Sextic       => InSextic,
                                          EasingAlgorithmType.Septic       => InSeptic,
                                          EasingAlgorithmType.Octic        => InOctic,
                                          EasingAlgorithmType.Nonic        => InNonic,
                                          EasingAlgorithmType.Decic        => InDecic,
                                          EasingAlgorithmType.Back         => InBack,
                                          EasingAlgorithmType.Bounce       => InBack,
                                          EasingAlgorithmType.Circle       => InBack,
                                          EasingAlgorithmType.Elastic      => InBack,
                                          _                                => Linear
                                      },
                EasingDirection.Out => algorithmType switch
                                       {
                                           EasingAlgorithmType.ConstantZero => ConstantZero,
                                           EasingAlgorithmType.ConstantOne  => ConstantOne,
                                           EasingAlgorithmType.Linear       => Linear,
                                           EasingAlgorithmType.Quadratic    => OutQuadratic,
                                           EasingAlgorithmType.Cubic        => OutCubic,
                                           EasingAlgorithmType.Quartic      => OutQuartic,
                                           EasingAlgorithmType.Quintic      => OutQuintic,
                                           EasingAlgorithmType.Sextic       => OutSextic,
                                           EasingAlgorithmType.Septic       => OutSeptic,
                                           EasingAlgorithmType.Octic        => OutOctic,
                                           EasingAlgorithmType.Nonic        => OutNonic,
                                           EasingAlgorithmType.Decic        => OutDecic,
                                           EasingAlgorithmType.Back         => OutBack,
                                           EasingAlgorithmType.Bounce       => OutBack,
                                           EasingAlgorithmType.Circle       => OutBack,
                                           EasingAlgorithmType.Elastic      => OutBack,
                                           _                                => Linear
                                       },
                EasingDirection.InOut => algorithmType switch
                                         {
                                             EasingAlgorithmType.ConstantZero => ConstantZero,
                                             EasingAlgorithmType.ConstantOne  => ConstantOne,
                                             EasingAlgorithmType.Linear       => Linear,
                                             EasingAlgorithmType.Quadratic    => InOutQuadratic,
                                             EasingAlgorithmType.Cubic        => InOutCubic,
                                             EasingAlgorithmType.Quartic      => InOutQuartic,
                                             EasingAlgorithmType.Quintic      => InOutQuintic,
                                             EasingAlgorithmType.Sextic       => InOutSextic,
                                             EasingAlgorithmType.Septic       => InOutSeptic,
                                             EasingAlgorithmType.Octic        => InOutOctic,
                                             EasingAlgorithmType.Nonic        => InOutNonic,
                                             EasingAlgorithmType.Decic        => InOutDecic,
                                             EasingAlgorithmType.Back         => InOutBack,
                                             EasingAlgorithmType.Bounce       => InOutBack,
                                             EasingAlgorithmType.Circle       => InOutBack,
                                             EasingAlgorithmType.Elastic      => InOutBack,
                                             _                                => Linear
                                         },
                _ => null
            };


        public static EaseMethod GetEasingMethod(EasingDirection     easingDirection,
                                                 EasingAlgorithmType algorithmType) =>
            algorithmType switch
            {
                EasingAlgorithmType.ConstantZero => ConstantZero,
                EasingAlgorithmType.ConstantOne  => ConstantOne,
                EasingAlgorithmType.Linear       => Linear,
                EasingAlgorithmType.Quadratic => easingDirection switch
                                                 {
                                                     EasingDirection.In    => InQuadratic,
                                                     EasingDirection.Out   => OutQuadratic,
                                                     EasingDirection.InOut => InOutQuadratic,
                                                     _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                                actualValue: easingDirection,
                                                                                                message: null)
                                                 },
                EasingAlgorithmType.Cubic => easingDirection switch
                                             {
                                                 EasingDirection.In    => InCubic,
                                                 EasingDirection.Out   => OutCubic,
                                                 EasingDirection.InOut => InOutCubic,
                                                 _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                            actualValue: easingDirection,
                                                                                            message: null)
                                             },
                EasingAlgorithmType.Quartic => easingDirection switch
                                               {
                                                   EasingDirection.In    => InQuartic,
                                                   EasingDirection.Out   => OutQuartic,
                                                   EasingDirection.InOut => InOutQuartic,
                                                   _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                              actualValue: easingDirection,
                                                                                              message: null)
                                               },
                EasingAlgorithmType.Quintic => easingDirection switch
                                               {
                                                   EasingDirection.In    => InQuintic,
                                                   EasingDirection.Out   => OutQuintic,
                                                   EasingDirection.InOut => InOutQuintic,
                                                   _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                              actualValue: easingDirection,
                                                                                              message: null)
                                               },
                EasingAlgorithmType.Sextic => easingDirection switch
                                              {
                                                  EasingDirection.In    => InSextic,
                                                  EasingDirection.Out   => OutSextic,
                                                  EasingDirection.InOut => InOutSextic,
                                                  _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                             actualValue: easingDirection,
                                                                                             message: null)
                                              },
                EasingAlgorithmType.Septic => easingDirection switch
                                              {
                                                  EasingDirection.In    => InSeptic,
                                                  EasingDirection.Out   => OutSeptic,
                                                  EasingDirection.InOut => InOutSeptic,
                                                  _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                             actualValue: easingDirection,
                                                                                             message: null)
                                              },
                EasingAlgorithmType.Octic => easingDirection switch
                                             {
                                                 EasingDirection.In    => InOctic,
                                                 EasingDirection.Out   => OutOctic,
                                                 EasingDirection.InOut => InOutOctic,
                                                 _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                            actualValue: easingDirection,
                                                                                            message: null)
                                             },
                EasingAlgorithmType.Nonic => easingDirection switch
                                             {
                                                 EasingDirection.In    => InNonic,
                                                 EasingDirection.Out   => OutNonic,
                                                 EasingDirection.InOut => InOutNonic,
                                                 _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                            actualValue: easingDirection,
                                                                                            message: null)
                                             },
                EasingAlgorithmType.Decic => easingDirection switch
                                             {
                                                 EasingDirection.In    => InDecic,
                                                 EasingDirection.Out   => OutDecic,
                                                 EasingDirection.InOut => InOutDecic,
                                                 _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                            actualValue: easingDirection,
                                                                                            message: null)
                                             },
                EasingAlgorithmType.Back => easingDirection switch
                                            {
                                                EasingDirection.In    => InBack,
                                                EasingDirection.Out   => OutBack,
                                                EasingDirection.InOut => InOutBack,
                                                _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                           actualValue: easingDirection,
                                                                                           message: null)
                                            },
                EasingAlgorithmType.Bounce => easingDirection switch
                                              {
                                                  EasingDirection.In    => InBack,
                                                  EasingDirection.Out   => OutBack,
                                                  EasingDirection.InOut => InOutBack,
                                                  _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                             actualValue: easingDirection,
                                                                                             message: null)
                                              },
                EasingAlgorithmType.Circle => easingDirection switch
                                              {
                                                  EasingDirection.In    => InBack,
                                                  EasingDirection.Out   => OutBack,
                                                  EasingDirection.InOut => InOutBack,
                                                  _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                             actualValue: easingDirection,
                                                                                             message: null)
                                              },
                EasingAlgorithmType.Elastic => easingDirection switch
                                               {
                                                   EasingDirection.In    => InBack,
                                                   EasingDirection.Out   => OutBack,
                                                   EasingDirection.InOut => InOutBack,
                                                   _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                                              actualValue: easingDirection,
                                                                                              message: null)
                                               },
                _ => Linear
            };


        #endregion

        // ---------------------------------------------------------------------------------------- Extension Easing //

        #region Extension easing (Self)



        public static void Ease(ref this float  input,
                                EasingDirection easingDirection,
                                int             polynomicStrength) =>
            input = polynomicStrength switch
                    {
                        0 => ConstantOne(p: input),
                        1 => Linear(t: input),
                        2 => easingDirection switch
                             {
                                 EasingDirection.In    => InQuadratic(t: input),
                                 EasingDirection.Out   => OutQuadratic(t: input),
                                 EasingDirection.InOut => InOutQuadratic(t: input),
                                 _                     => input
                             },
                        3 => easingDirection switch
                             {
                                 EasingDirection.In    => InCubic(t: input),
                                 EasingDirection.Out   => OutCubic(t: input),
                                 EasingDirection.InOut => InOutCubic(t: input),
                                 _                     => input
                             },
                        4 => easingDirection switch
                             {
                                 EasingDirection.In    => InQuartic(t: input),
                                 EasingDirection.Out   => OutQuartic(t: input),
                                 EasingDirection.InOut => InOutQuartic(t: input),
                                 _                     => input
                             },
                        5 => easingDirection switch
                             {
                                 EasingDirection.In    => InQuintic(t: input),
                                 EasingDirection.Out   => OutQuintic(t: input),
                                 EasingDirection.InOut => InOutQuintic(t: input),
                                 _                     => input
                             },
                        6 => easingDirection switch
                             {
                                 EasingDirection.In    => InSextic(t: input),
                                 EasingDirection.Out   => OutSextic(t: input),
                                 EasingDirection.InOut => InOutSextic(t: input),
                                 _                     => input
                             },
                        7 => easingDirection switch
                             {
                                 EasingDirection.In    => InSeptic(t: input),
                                 EasingDirection.Out   => OutSeptic(t: input),
                                 EasingDirection.InOut => InOutSeptic(t: input),
                                 _                     => input
                             },
                        8 => easingDirection switch
                             {
                                 EasingDirection.In    => InOctic(t: input),
                                 EasingDirection.Out   => OutOctic(t: input),
                                 EasingDirection.InOut => InOutOctic(t: input),
                                 _                     => input
                             },
                        9 => easingDirection switch
                             {
                                 EasingDirection.In    => InNonic(t: input),
                                 EasingDirection.Out   => OutNonic(t: input),
                                 EasingDirection.InOut => InOutNonic(t: input),
                                 _                     => input
                             },
                        10 => easingDirection switch
                              {
                                  EasingDirection.In    => InDecic(t: input),
                                  EasingDirection.Out   => OutDecic(t: input),
                                  EasingDirection.InOut => InOutDecic(t: input),
                                  _                     => input
                              },
                        _ => input
                    };



        #endregion

        #region Extension easing (Return)



        public static float GetEased
        (
            this float      input,
            EasingDirection easingDirection,
            int             polynomicStrength) =>
            polynomicStrength switch
            {
                0 => ConstantOne(p: input),
                1 => Linear(t: input),
                2 => easingDirection switch
                     {
                         EasingDirection.In    => InQuadratic(t: input),
                         EasingDirection.Out   => OutQuadratic(t: input),
                         EasingDirection.InOut => InOutQuadratic(t: input),
                         _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                    actualValue: easingDirection,
                                                                    message: null)
                     },
                3 => easingDirection switch
                     {
                         EasingDirection.In    => InCubic(t: input),
                         EasingDirection.Out   => OutCubic(t: input),
                         EasingDirection.InOut => InOutCubic(t: input),
                         _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                    actualValue: easingDirection,
                                                                    message: null)
                     },
                4 => easingDirection switch
                     {
                         EasingDirection.In    => InQuartic(t: input),
                         EasingDirection.Out   => OutQuartic(t: input),
                         EasingDirection.InOut => InOutQuartic(t: input),
                         _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                    actualValue: easingDirection,
                                                                    message: null)
                     },
                5 => easingDirection switch
                     {
                         EasingDirection.In    => InQuintic(t: input),
                         EasingDirection.Out   => OutQuintic(t: input),
                         EasingDirection.InOut => InOutQuintic(t: input),
                         _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                    actualValue: easingDirection,
                                                                    message: null)
                     },
                6 => easingDirection switch
                     {
                         EasingDirection.In    => InSextic(t: input),
                         EasingDirection.Out   => OutSextic(t: input),
                         EasingDirection.InOut => InOutSextic(t: input),
                         _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                    actualValue: easingDirection,
                                                                    message: null)
                     },
                7 => easingDirection switch
                     {
                         EasingDirection.In    => InSeptic(t: input),
                         EasingDirection.Out   => OutSeptic(t: input),
                         EasingDirection.InOut => InOutSeptic(t: input),
                         _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                    actualValue: easingDirection,
                                                                    message: null)
                     },
                8 => easingDirection switch
                     {
                         EasingDirection.In    => InOctic(t: input),
                         EasingDirection.Out   => OutOctic(t: input),
                         EasingDirection.InOut => InOutOctic(t: input),
                         _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                    actualValue: easingDirection,
                                                                    message: null)
                     },
                9 => easingDirection switch
                     {
                         EasingDirection.In    => InNonic(t: input),
                         EasingDirection.Out   => OutNonic(t: input),
                         EasingDirection.InOut => InOutNonic(t: input),
                         _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                    actualValue: easingDirection,
                                                                    message: null)
                     },
                10 => easingDirection switch
                      {
                          EasingDirection.In    => InDecic(t: input),
                          EasingDirection.Out   => OutDecic(t: input),
                          EasingDirection.InOut => InOutDecic(t: input),
                          _ => throw new ArgumentOutOfRangeException(paramName: nameof(easingDirection),
                                                                     actualValue: easingDirection,
                                                                     message: null)
                      },
                _ => Linear(t: input)
            };



        #endregion

        // ------------------------------------------------------------------------------------------ Easing Methods //

        #region Constant Methods



        /// <summary>
        ///     Returns a polynomial degree of 0, i.e. nothing.
        /// </summary>
        public static float ConstantZero(this float p) => 0;


        /// <summary>
        ///     Returns a polynomial degree of 0, i.e. nothing.
        /// </summary>
        public static float ConstantOne(this float p) => 1;



        #endregion

        // -------------------------------------------------------------------------------------------------- Linear //

        #region Linear Methods



        /// <summary>
        ///     Returns a polynomial degree of 1, i.e. the exact same value.
        /// </summary>
        public static float Linear(this float t) => t;

        public static float Step(float t) => (double) t < 0.5 ? 0.0f : 1f;

        #endregion
        
        // ---------------------------------------------------------------------------------------------------------------------------------- Sin //
        
        public static float InSine(float    t) => Mathf.Sin((float) (halfPi * (t - 1.0))) + 1f;
        public static float OutSine(float   t) => Mathf.Sin(t * halfPi);
        public static float InOutSine(float t) => (float) (((double) Mathf.Sin((float) (Mathf.PI * ((double) t - 0.5))) + 1.0) * 0.5);
        
        // ----------------------------------------------------------------------------------------------------------------------------- Power Of //

        public static float InPower(float t,
                                    float   power) => Mathf.Pow(f: t,
                                                                p: power);

        public static float OutPower(float t,
                                     float power) => 1.0f - Mathf.Pow(f: 1.0f - t,
                                                                      p: power);


        public static float InOutPower(float t,
                                       float power) =>
            t < 0.5f ?
                Mathf.Pow(f: t * 2.0f,
                          p: power) * 0.5f :
                1.0f - Mathf.Pow(f: 2.0f - t * 2.0f,
                                 p: power) * 0.5f;

        // ------------------------------------------------------------------------------------------------------------------------------- Bounce //

        public static float Bounce(float t, float power) =>
            1.0f -
            Mathf.Pow(f: 1.0f - (-0.5f + t * 2.0f),
                      p: power) * 2.0f;

        // ------------------------------------------------------------------------------------------------------ In //

        #region In Methods - 'Basic'



        // Basic //

//        public static float InSine(float    t) => Mathf.Sin((float) (1.57079637050629 * (t - 1.0))) + 1f;
//        public static float OutSine(float   t) => Mathf.Sin(t * 1.570796f);
//        public static float InOutSine(float t) => (float) (((double) Mathf.Sin((float) (3.14159274101257 * ((double) t - 0.5))) + 1.0) * 0.5);

        /// <summary>
        ///     Ease in with a polynomial degree of 2
        /// </summary>
        public static float InQuadratic(this float t) => t * t;


        /// <summary>
        ///     Ease in with a polynomial degree of 3
        /// </summary>
        public static float InCubic(this float t) => t * t * t;


        /// <summary>
        ///     Ease in with a polynomial degree of 4
        /// </summary>
        public static float InQuartic(this float t) => t * t * t * t;


        /// <summary>
        ///     Ease in with a polynomial degree of 5
        /// </summary>
        public static float InQuintic(this float t) => t * t * t * t * t;


        /// <summary>
        ///     Ease in with a polynomial degree of 6
        /// </summary>
        public static float InSextic(this float t) => t * t * t * t * t * t;


        /// <summary>
        ///     Ease in with a polynomial degree of 7
        /// </summary>
        public static float InSeptic(this float t) => t * t * t * t * t * t * t;


        /// <summary>
        ///     Ease in with a polynomial degree of 8
        /// </summary>
        public static float InOctic(this float t) => t * t * t * t * t * t * t * t;


        /// <summary>
        ///     Ease in with a polynomial degree of 9
        /// </summary>
        public static float InNonic(this float t) => t * t * t * t * t * t * t * t * t;


        /// <summary>
        ///     Ease in with a polynomial degree of 10
        /// </summary>
        public static float InDecic(this float t) => t * t * t * t * t * t * t * t * t * t;



        #endregion

        #region In Methods - 'FX'



        // FX //


        /// <summary>
        ///     Ease in by dipping underneath the min value deliberately by 10%.
        /// </summary>
        public static float InBack(this float t) => (float) ( t * t * ( t * ( k + 1 ) - k ) );


        /// <summary>
        ///     Ease in with a bounce-like animation.
        /// </summary>
        public static float InBounce(this float t) => 1 - OutBounce(t: 1 - t);


        /// <summary>
        ///     Ease in with the curvature of a perfect circle.
        /// </summary>
        public static float InCircle(this float t) => 1 - Mathf.Sqrt(f: 1 - t * t);

        /// <summary>
        ///     Ease out with the curvature of a perfect circle.
        /// </summary>
        public static float OutCircle
        (
            this float t)
        {
            var m = t - 1;

            return Mathf.Sqrt(f: 1 - m * m);
        }
        
        /// <summary>
        ///     Ease in and then out with the curvature of a perfect circle.
        /// </summary>
        public static float InOutCircle
        (
            this float t)
        {
            var m = t - 1;

            var p = t * 2;

            return (float) ( p < 1 ?
                                 ( 1 - Mathf.Sqrt(f: 1 - p * p) )         * 0.5 :
                                 ( Mathf.Sqrt(f: 1     - 4 * m * m) + 1 ) * 0.5 );
        }


        /// <summary>
        ///     Ease in with an elastic wobble.
        /// </summary>
        public static float InElastic
        (
            this float t)
        {
            var m = t - 1;

            return -Mathf.Pow(f: 2,
                              p: 10 * m) *
                   Mathf.Sin(f: ( m * 40 - 3 ) * Mathf.PI / 6);
        }



        #endregion

        // ----------------------------------------------------------------------------------------------------- Out //

        #region Out Methods - 'Basic'



        // Basic //


        /// <summary>
        ///     Ease out with a polynomial degree of 2
        /// </summary>
        public static float OutQuadratic
        (
            this float t)
        {
            var m = t - 1;

            return 1 - m * m;
        }


        /// <summary>
        ///     Ease out with a polynomial degree of 3
        /// </summary>
        public static float OutCubic
        (
            this float t)
        {
            var m = t - 1;

            return 1 + m * m * m;
        }


        /// <summary>
        ///     Ease out with a polynomial degree of 4
        /// </summary>
        public static float OutQuartic
        (
            this float t)
        {
            var m = t - 1;

            return 1 - m * m * m * m;
        }


        /// <summary>
        ///     Ease out with a polynomial degree of 5
        /// </summary>
        public static float OutQuintic
        (
            this float t)
        {
            var m = t - 1;

            return 1 + m * m * m * m * m;
        }


        /// <summary>
        ///     Ease out with a polynomial degree of 6
        /// </summary>
        public static float OutSextic
        (
            this float t)
        {
            var m = t - 1;

            return 1 - m * m * m * m * m * m;
        }


        /// <summary>
        ///     Ease out with a polynomial degree of 7
        /// </summary>
        public static float OutSeptic
        (
            this float t)
        {
            var m = t - 1;

            return 1 + m * m * m * m * m * m * m;
        }


        /// <summary>
        ///     Ease out with a polynomial degree of 8
        /// </summary>
        public static float OutOctic
        (
            this float t)
        {
            var m = t - 1;

            return 1 - m * m * m * m * m * m * m * m;
        }


        /// <summary>
        ///     Ease out with a polynomial degree of 9
        /// </summary>
        public static float OutNonic
        (
            this float t)
        {
            var m = t - 1;

            return 1 + m * m * m * m * m * m * m * m * m;
        }


        /// <summary>
        ///     Ease out with a polynomial degree of 10
        /// </summary>
        public static float OutDecic
        (
            this float t)
        {
            var m = t - 1;

            return 1 + m * m * m * m * m * m * m * m * m;
        }



        #endregion

        #region Out Methods - 'FX'



        // FX //


        /// <summary>
        ///     Ease out by overshooting our max value deliberately by 10%.
        /// </summary>
        public static float OutBack
        (
            this float t)
        {
            var m = t - 1;

            return (float) ( 1 + m * m * ( m * ( k + 1 ) + k ) );
        }


        /// <summary>
        ///     Ease out with a bounce-like animation.
        /// </summary>
        public static float OutBounce
        (
            this float t)
        {
            #region Old Mode



//            var k1 = r; // 36.36%
//            var k2 = 2 * r; // 72.72%
//            var k3 = 1.5 * r; // 54.54%
//            var k4 = 2.5 * r; // 90.90%
//            var k5 = 2.25 * r; // 81.81%
//            var k6 = 2.625 * r; // 95.45%
//            var k0 = 7.5625;

//            if (p < k1)
//            {
//                return (float) (k0 * p * p);
//            }
//
//            if (p < k2)
//            {
//                // 48 / 64
//                
//                t = p - k3;
//
//                return (float) (k0 * t * t + 0.75);
//            }
//
//            if (p < k4)
//            {
//                // 60 / 64
//                
//                t = p - k5;
//                
//                return (float) (k0 * t * t + 0.9375);
//            }
//
//            // 63 / 64
//
//            t = p - k6;
//            
//            return (float) (k0 * t * t + 0.984375);



            #endregion

            double p;

            if (t < r)
            {
                return (float) ( l0 * t * t );
            }

            var l1 = 2   * r; // 72.72%
            var l2 = 1.5 * r; // 54.54%

            if (t < l1)
            {
                // 48 / 64

                p = t - l2;

                return (float) ( l0 * p * p + 0.75 );
            }

            l1 = 2.5  * r; // 90.90%
            l2 = 2.25 * r; // 81.81%

            if (t < l1)
            {
                // 60 / 64

                p = t - l2;

                return (float) ( l0 * p * p + 0.9375 );
            }

            l1 = 2.625 * r; // 95.45%

            // 63 / 64

            p = t - l1;

            return (float) ( l0 * p * p + 0.984375 );
        }





        /// <summary>
        ///     Ease out with an elastic wobble.
        /// </summary>
        public static float OutElastic(this float t) =>
            1 +
            Mathf.Pow(f: 2,
                      p: 10 * -t) *
            Mathf.Sin(f: (-t * 40 - 3) * Mathf.PI / 6);



        #endregion

        // ------------------------------------------------------------------------------------------------ In / Out //

        #region In/Out Methods - 'Basic'



        // Basic //


        /// <summary>
        ///     Ease in and out with a polynomial degree of 2
        /// </summary>
        public static float InOutQuadratic
        (
            this float t)
        {
            var m = t - 1;

            var p = t * 2;

            if (p < 1) return t * p;

            return 1 - m * m * 2;
        }


        /// <summary>
        ///     Ease in and out with a polynomial degree of 3
        /// </summary>
        public static float InOutCubic
        (
            this float t)
        {
            var m = t - 1;

            var p = t * 2;

            if (p < 1) return t * p * p;

            return 1 + m * m * m * 4;
        }


        /// <summary>
        ///     Ease in and out with a polynomial degree of 4
        /// </summary>
        public static float InOutQuartic
        (
            this float t)
        {
            var m = t - 1;

            var p = t * 2;

            if (p < 1) return t * p * p * p;

            return 1 - m * m * m * m * 8;
        }


        /// <summary>
        ///     Ease in and out with a polynomial degree of 5
        /// </summary>
        public static float InOutQuintic
        (
            this float t)
        {
            var m = t - 1;

            var p = t * 2;

            if (p < 1) return t * p * p * p * p;

            return 1 + m * m * m * m * m * 16;
        }


        /// <summary>
        ///     Ease in and out with a polynomial degree of 6
        /// </summary>
        public static float InOutSextic
        (
            this float t)
        {
            var m = t - 1;

            var p = t * 2;

            if (p < 1) return t * p * p * p * p * p;

            return 1 - m * m * m * m * m * m * 32;
        }


        /// <summary>
        ///     Ease in and out with a polynomial degree of 7
        /// </summary>
        public static float InOutSeptic
        (
            this float t)
        {
            var m = t - 1;

            var p = t * 2;

            if (p < 1) return t * p * p * p * p * p * p;

            return 1 + m * m * m * m * m * m * m * 64;
        }


        /// <summary>
        ///     Ease in and out with a polynomial degree of 8
        /// </summary>
        public static float InOutOctic
        (
            this float t)
        {
            var m = t - 1;

            var p = t * 2;

            if (p < 1) return t * p * p * p * p * p * p * p;

            return 1 - m * m * m * m * m * m * m * m * 128;
        }


        /// <summary>
        ///     Ease in and out with a polynomial degree of 9
        /// </summary>
        public static float InOutNonic
        (
            this float t)
        {
            var m = t - 1;

            var p = t * 2;

            if (p < 1) return t * p * p * p * p * p * p * p * p;

            return 1 + m * m * m * m * m * m * m * m * m * 256;
        }


        /// <summary>
        ///     Ease in and out with a polynomial degree of 10
        /// </summary>
        public static float InOutDecic
        (
            this float t)
        {
            var m = t - 1;

            var p = t * 2;

            if (p < 1) return t * p * p * p * p * p * p * p * p * p;

            return 1 - m * m * m * m * m * m * m * m * m * m * 512;
        }



        #endregion

        #region In/Out Methods - 'FX'



        // FX //


        /// <summary>
        ///     Ease in and out by dipping under and then overshooting our min and max values respectively by 10%.
        /// </summary>
        public static float InOutBack
        (
            this float t)
        {
            var m = t - 1;

            var p = t * 2;

            return (float) ( t < 0.5f ?
                                 t * p * ( p * ( n + 1 ) - n ) :
                                 1 + 2 * m * m * ( 2 * m * ( n + 1 ) + n ) );
        }


        /// <summary>
        ///     Ease in and out with a bounce-like animation.
        /// </summary>
        public static float InOutBounce
        (
            this float t)
        {
            var p = t * 2;

            return (float) ( p < 1 ?
                                 0.5 - 0.5 * OutBounce(t: 1 - p) :
                                 0.5 + 0.5 * OutBounce(t: p - 1) );
        }





        /// <summary>
        ///     Ease in and out with an elastic wobble.
        /// </summary>
        public static float InOutElastic
        (
            this float t)
        {
            var s = 2              * t - 1;         // remap: [0.0, 0.5] -> [-1.0, 0.0]
            var l = ( 80 * s - 9 ) * Mathf.PI / 18; // and    [0.5, 1.0] -> [0.0, 1.0]

            return (float) ( s < 0 ?
                                 -0.5 *
                                 Mathf.Pow(f: 2,
                                           p: 10 * s) *
                                 Mathf.Sin(f: l) :
                                 1 +
                                 0.5 *
                                 Mathf.Pow(f: 2,
                                           p: -10 * s) *
                                 Mathf.Sin(f: l) );
        }

        #endregion

    }

}