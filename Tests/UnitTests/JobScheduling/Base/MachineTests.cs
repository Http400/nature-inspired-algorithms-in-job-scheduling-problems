using System.Collections.Generic;
using Core.JobScheduling.Base;
using Core.Utils;
using NUnit.Framework;

namespace Tests.UnitTests.Base
{
    [TestFixture]
    public class MachineTests
    {
        [Test, TestCaseSource("adding_operation_test_cases")]
        public void test_adding_operation(Machine testMachine, Operation testOperation, int testOperationStartTime, List<Machine.TimeGap> expectedTimeGaps)
        {
            // Arrange
            var machine = testMachine;
            var newOperation = testOperation;
            
            // Act
            machine.AddOperation(newOperation, testOperationStartTime);

            // Assert
            Assert.IsTrue( machine.TimeGaps.IsEqual(expectedTimeGaps) );
        }

        private static object[] adding_operation_test_cases = 
        {
            new object[] {
                new Machine(0) {
                    Operations = new List<(Operation Operation, int StartTime)>() {
                        ( new Operation() { ProcessingTime = 3 }, 1),
                        ( new Operation() { ProcessingTime = 2 }, 6),
                        ( new Operation() { ProcessingTime = 1 }, 9),
                        ( new Operation() { ProcessingTime = 4 }, 14)
                    },
                    TimeGaps = new List<Machine.TimeGap>() { 
                        new Machine.TimeGap(4, 2),
                        new Machine.TimeGap(8, 1),
                        new Machine.TimeGap(10, 4)
                    }
                },
                new Operation() { ProcessingTime = 2 }, 11,
                new List<Machine.TimeGap>() { 
                    new Machine.TimeGap(4, 2),
                    new Machine.TimeGap(8, 1),
                    new Machine.TimeGap(10, 1),
                    new Machine.TimeGap(13, 1)
                }
            },
            new object[] {
                new Machine(1) {
                    Operations = new List<(Operation Operation, int StartTime)>(),
                    TimeGaps = new List<Machine.TimeGap>()
                },
                new Operation() { ProcessingTime = 2 }, 1,
                new List<Machine.TimeGap>() { 
                    new Machine.TimeGap(0, 1)
                }
            },
            new object[] {
                new Machine(2) {
                    Operations = new List<(Operation Operation, int StartTime)>() {
                        ( new Operation() { ProcessingTime = 3 }, 0),
                        ( new Operation() { ProcessingTime = 2 }, 3)
                    },
                    TimeGaps = new List<Machine.TimeGap>()
                },
                new Operation() { ProcessingTime = 4 }, 6,
                new List<Machine.TimeGap>() { 
                    new Machine.TimeGap(5, 1)
                }
            },
            new object[] {
                new Machine(3) {
                    Operations = new List<(Operation Operation, int StartTime)>() {
                        ( new Operation() { ProcessingTime = 3 }, 0),
                        ( new Operation() { ProcessingTime = 2 }, 3)
                    },
                    TimeGaps = new List<Machine.TimeGap>()
                },
                new Operation() { ProcessingTime = 4 }, 5,
                new List<Machine.TimeGap>()
            }
        };
    }
}