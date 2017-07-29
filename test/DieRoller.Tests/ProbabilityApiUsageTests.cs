﻿using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DieRoller.Tests
{
    /// <summary>
    ///     This test class will use the public fluent <see cref="RollBuilder" /> api and validate expected calculations.
    /// </summary>
    public class ProbabilityApiUsageTests
    {
        private readonly ITestOutputHelper _output;

        public ProbabilityApiUsageTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public void GivenD6_NoReroll_NoModifier(int valueAndAboveTarget)
        {
            var roll = RollBuilder.WithDie(Die.D6)
                .Targeting(Target.ValueAndAbove(valueAndAboveTarget))
                .Build();

            // Calc - SuccessfulSideCount / Sides
            var expectedProbability = (7 - valueAndAboveTarget) / 6m;
            CheckProbability(roll, expectedProbability);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public void GivenD6_RerollFailures(int valueAndAboveTarget)
        {
            var die = Die.D6;
            var roll = RollBuilder.WithDie(die)
                .Targeting(Target.ValueAndAbove(valueAndAboveTarget))
                .WithReroll(Reroll.Failures)
                .Build();

            // Calc - (SuccessfulSideCount / Sides) + ((InverseOfSuccessfulSideCount / Sides) * (SuccessfulSideCount / Sides))
            var decimalSides = (decimal)die.Sides;
            var successfulSideCount = die.Sides + 1 - valueAndAboveTarget;
            var expectedProbability = successfulSideCount / decimalSides + (die.Sides - successfulSideCount) / decimalSides * (successfulSideCount / decimalSides);
            CheckProbability(roll, expectedProbability);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public void GivenD6_RerollOnes(int valueAndAboveTarget)
        {
            var die = Die.D6;
            var roll = RollBuilder.WithDie(die)
                .Targeting(Target.ValueAndAbove(valueAndAboveTarget))
                .WithReroll(Reroll.Ones)
                .Build();

            // Calc - (SuccessfulSideCount / Sides) + ((1 / Sides) * (SuccessfulSideCount / Sides))
            var decimalSides = (decimal)die.Sides;
            var successfulSideCount = die.Sides + 1 - valueAndAboveTarget;
            var expectedProbability = successfulSideCount / decimalSides + 1 / decimalSides * (successfulSideCount / decimalSides);
            CheckProbability(roll, expectedProbability);
        }

        [Fact]
        public void GivenD6_RerollOnes_WhenOneIsSuccess()
        {
            var die = Die.D6;
            var roll = RollBuilder.WithDie(die)
                .Targeting(Target.ValueAndAbove(1))
                .WithReroll(Reroll.Ones)
                .Build();

            // Calc - If the base probability is already 100%, reroll should not increase it further as you should not be rerolling.
            var expectedProbability = 1m;
            CheckProbability(roll, expectedProbability);
        }

        private void CheckProbability(Roll roll, decimal expectedProbability)
        {
            var actualProbability = roll.CalculateProbability();
            _output.WriteLine($"Probability: {actualProbability:P}");
            actualProbability.Should().Be(expectedProbability);
        }
    }
}