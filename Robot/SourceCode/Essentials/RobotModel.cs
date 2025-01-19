using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot.Essentials
{
    internal class RobotModel
    {
        public BitOperations bitOperations { get; set; }
        public PreciseDelay preciseDelay { get; set; }
        public Board board { get; set; }

        public FileOperations fileOperations { get; set; }
        public Form1 form { get; set; }

        public bool recording = false;

        public RobotModel(Form1 formParam)
        {
            // assing the global objects
            board = new Board();
            bitOperations = new BitOperations();
            preciseDelay = new PreciseDelay();
            fileOperations = new FileOperations();

            // set the local form var to the received form
            form = formParam;
        }

        public void Step(char motor, char dir)
        {
            // function for performing one step with the motors in specified direction
            // set out port as default for this methid
            int port = RobotWiring.Out.PORT;

            byte output = 0xFF; // initial state of the port


            // set direction
            if (dir == 'R')
            {
                // moving right/up
                output = bitOperations.NullBit(output, RobotWiring.GetPinID(RobotWiring.Out.DIR), true);
            }
            else
            {
                // moving left/down
                output = bitOperations.SetBit(output, RobotWiring.GetPinID(RobotWiring.Out.DIR), true);
            }

            // select base motor
            if (motor == 'B')
            {
                // BASE motor
                output = bitOperations.SetBit(output, RobotWiring.GetPinID(RobotWiring.Out.BASE), true);
            }
            else if (motor == 'A')
            {
                // ARM motor
                output = bitOperations.SetBit(output, RobotWiring.GetPinID(RobotWiring.Out.ARM), true);
            }
            else if (motor == 'N')
            {
                // NECK motor
                output = bitOperations.SetBit(output, RobotWiring.GetPinID(RobotWiring.Out.NECK), true);
            }
            else if(motor == 'G')
            {
                // GRIP motor
                output = bitOperations.SetBit(output, RobotWiring.GetPinID(RobotWiring.Out.GRIP), true);
            };


            // set the motor and direction pins accordingly
            board.SetByte(port, output);

            // tick the clock to make the step happen
            Tick(port);
        }

        public void Move(char motor, char dir, int modeID)
        {
            // function moving in steps and saving the moves to a file

            // for each move -> a new line with the current mode, motor, direction and steps
            // probaby a csv file or a custom file spearazed by | -> easy to distinquish
            // need a writer and a reader
            // function as an interpreter to move as the file says so
            // the recording will be started by a button pres

            // show the info to the screen
            form.ShowMotorInfoData(motor, dir);

            // calculate the number of steps based on the current mode of operation
            int n = (modeID == 0) ? RobotWiring.General.STRADA_STEP : (modeID == 1) ? RobotWiring.General.SPORT_STEP : RobotWiring.General.CORSA_STEP;
            for(int i = 0; i < n; i++)
            {
                if (recording)
                {
                    // if movements are to be recorded -> write them to a file
                    fileOperations.Write(modeID, motor, dir);
                }
                Step(motor, dir);
            }
        }
        
        public void MoveByLine(string file)
        {
            // function for movinf with the robot accordingly to the read data
            // initialize the motors to always start at some reference point
            InitializeMotors();

            // make sure that the movements are not recorded again
            recording = false;  
            foreach (string[] movementData in fileOperations.ReadFile(file))
            {
                // for each line in the file
                int mode;
                char motor, direction;
             
                mode = Int32.Parse(movementData[0]);
                motor = char.Parse(movementData[1]);
                direction = char.Parse(movementData[2]);
                Move(motor, direction, mode);
            }
        }

        public void InitializeMotors()
        {
            // function for getting the robot into init position
            // set the default state to disabled
           
            form.Invoke(new Action(() =>
                form.ShowMotorInitData(false, false, false, false)
            ));

            // input port
            int port = RobotWiring.In.PORT;


            // BASE motor inicialization
            // first phase of base init -> spin right for a set number of steps

            for (int i = 0; i < RobotWiring.General.BASE_CROSS_STEPS; i++)
            {
                Step('B', 'R'); // spin right

                if (!board.GetBit(port, RobotWiring.GetPinID(RobotWiring.In.BASE_IR)))
                {
                    // if initialized break aout of the loop 
                    break;
                }
            }


            // the first phase of base init was not successfull -> spin in the other direction
            while (board.GetBit(port, RobotWiring.GetPinID(RobotWiring.In.BASE_IR)))
            {
                Step('B', 'L'); // spin left
            }

            // edit the form in a different thread
            form.Invoke(new Action(() =>
                form.ShowMotorInitData(true, false, false, false)
            ));

            // ARM motor inicialization
            // get the state of the IR gate -> if high -> already set; if not go up
            // while should do the job
            while (board.GetBit(port, RobotWiring.GetPinID(RobotWiring.In.ARM_IR)))
            {
                // move up till the gate is not crossed
                Step('A', 'L');
            }

            // edit the form in a different thread
            form.Invoke(new Action(() =>
                form.ShowMotorInitData(true, true, false, false)
            ));

            // NECK motor inicialization
            while (board.GetBit(port, RobotWiring.GetPinID(RobotWiring.In.NECK_IR)))
            {
                // move up while the gate is not croseed;
                Step('N', 'R');
            }

            // edit the form in a different thread
            form.Invoke(new Action(() =>
                form.ShowMotorInitData(true, true, true, false)
            ));

            //GRIP motor inicialization
            while (board.GetBit(port, RobotWiring.GetPinID(RobotWiring.In.GRIP_IR)))
            {
                // while the gate is not crossed open the gripper
                Step('G', 'L');
            }

            //edit the form in a different thread
            form.Invoke(new Action(() =>
                form.ShowMotorInitData(true, true, true, true)
            ));
        }

        public void Tick(int port)
        {
            // function for providing motors with clock pulse
            // get the value of the whole output port when called
            byte portOnCall = board.GetPortState(port);

            // high clock pulse
            // perform an and operation to only change the bits for clock
            byte val = (byte)(portOnCall & bitOperations.InvertByte(RobotWiring.Out.TACT));
            board.SetByte(port, val);

            // wait for the motor to react
            preciseDelay.DelayMicroseconds(RobotWiring.General.HALF_DELAY);

            // get the state of the port after the first clock pulse
            portOnCall = board.GetPortState(port);

            // invert the previous value on the TACT pin by nulling it
            board.SetByte(port, bitOperations.NullBit(portOnCall, RobotWiring.GetPinID(RobotWiring.Out.TACT), true));

            // wait for the motor to react
            preciseDelay.DelayMicroseconds(RobotWiring.General.HALF_DELAY);
        }
    }
}
