using Ammeter;
using FrameworkCore.Metadata.DeviceDefine;
using FrameworkCore.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usr_ammeter
{
    class AmmeterCommandTimes
    {
        public AmmeterCommand AmmeterCommand;
        public int TrySendTimes;
    }
    class CommandAssisant
    {
        public RedisCommand RedisCommand;
        public List<AmmeterCommandTimes> AmmeterCommandLists;
    }

    public class UsrGatewayProtocolConvert
    {
        internal int TrySendTimes { get; set; }
        internal string GatewayId { get; set; }
        internal string GateWayServerId { get; set; }
        readonly Dictionary<string, Ammeter> _ammeters = new Dictionary<string, Ammeter>();

        CommandAssisant CurrentCommand;
        readonly Queue<CommandAssisant> AllCommands = new Queue<CommandAssisant>();

        public async Task InitAsync()
        {
            foreach (var ammeter in await UsrHelper.GetAmmetersAsync(GatewayId))
            {
                _ammeters.Add(ammeter.AmmeterId, ammeter);
            }
        }

        public void AddRedisCommand(RedisCommand redisCommand)
        {
            CommandAssisant commandAssisant = new CommandAssisant()
            {
                AmmeterCommandLists = new List<AmmeterCommandTimes>(),
                RedisCommand = redisCommand
            };

            IEnumerable<AmmeterCommand> ammeterCommands = ParseRedisCommand(redisCommand);
            foreach (var item in ammeterCommands)
            {
                commandAssisant.AmmeterCommandLists.Add(
                    new AmmeterCommandTimes() { AmmeterCommand = item, TrySendTimes = TrySendTimes });
            }

            if (CurrentCommand == null)
            {
                CurrentCommand = commandAssisant;
            }
            else
            {
                AllCommands.Enqueue(commandAssisant);
            }
        }

        #region ParseRedisCommand
        private IEnumerable<AmmeterCommand> ParseRedisCommand(RedisCommand redisCommand)
        {
            IEnumerable<AmmeterCommand> ret = null;
            switch (redisCommand.CommandType)
            {
                case CommandType.GetProperties:
                    ret = ParseGetProperties(redisCommand);
                    break;
                case CommandType.SetProperties:
                    ret = ParseSetProperties(redisCommand);
                    break;
                case CommandType.Function:
                    ret = ParseFunction(redisCommand);
                    break;
                case CommandType.EventReport:
                    break;
                default:
                    break;
            }
            return ret;
        }

        private IEnumerable<AmmeterCommand> ParseFunction(RedisCommand redisCommand)
        {
            throw new NotImplementedException();

        }

        private IEnumerable<AmmeterCommand> ParseSetProperties(RedisCommand redisCommand)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<AmmeterCommand> ParseGetProperties(RedisCommand redisCommand)
        {
            List<AmmeterCommand> commands = new List<AmmeterCommand>();
            foreach (var item in redisCommand.Request)
            {
                switch (item.Key)
                {
                    case "电量":
                        commands.Add(new AmmeterCommand()
                        {
                            MeterAddress = redisCommand.MacAddress,
                            _command = new GetAmmeterEnergy()
                        });
                        break;
                    case "功率":
                        commands.Add(new AmmeterCommand()
                        {
                            MeterAddress = redisCommand.MacAddress,
                            _command = new GetAmmeterPower()
                        });
                        break;
                    default:
                        break;
                }
            }
            return commands;
        }
        #endregion ParseRedisCommand

        internal AmmeterCommand GetNextAmmeterCommand()
        {
            if (CurrentCommand == null)
            {
                return null;
            }
            return CurrentCommand.AmmeterCommandLists[0].AmmeterCommand;
        }

        internal RedisCommand MatchCommand(AmmeterCommand command)
        {
            if (command == null)
            {
                if (CurrentCommand == null)         //设备挺悠闲，没事干
                {
                    return null;
                }
                else
                {
                    if (CurrentCommand.AmmeterCommandLists[0].TrySendTimes <= 0)   //设备不回应次数超上限
                    {
                        CurrentCommand.RedisCommand.Response.Add(new KeyValuePair<string, string>(
                            CurrentCommand.AmmeterCommandLists[0].AmmeterCommand._command.ToString(),
                            $"超时{TrySendTimes}次"));

                        CurrentCommand.AmmeterCommandLists.RemoveAt(0);
                        if (CurrentCommand.AmmeterCommandLists.Count == 0)
                        {
                            RedisCommand ret = CurrentCommand.RedisCommand;
                            CurrentCommand = null;
                            AllCommands.TryDequeue(out CurrentCommand);
                            return ret;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                if (command.NeedMatchCommand)
                {
                    if (command.MatchCommand(CurrentCommand.AmmeterCommandLists[0].AmmeterCommand))
                    {
                        ParseAmmeterCommand(CurrentCommand.RedisCommand, command);

                        CurrentCommand.AmmeterCommandLists.RemoveAt(0);
                        if (CurrentCommand.AmmeterCommandLists.Count == 0)
                        {
                            RedisCommand ret = CurrentCommand.RedisCommand;
                            CurrentCommand = null;
                            AllCommands.TryDequeue(out CurrentCommand);
                            return ret;
                        }
                    }
                    else
                    {
                        //这种情况太奇怪了
                    }
                }
                else
                {
                    //设备主动发的指令
                    RedisCommand response = new RedisCommand()
                    {
                        CommandId = Guid.NewGuid().ToString(),
                        GatewayId = GatewayId,
                        GateWayServerId = GateWayServerId,
                        ApplicateServerId = RedisService.CommonApplicateServer,
                        CommandType = CommandType.EventReport,
                        MacAddress = command.MeterAddress,
                        DeviceId = _ammeters[command.MeterAddress].AmmeterId,
                        Response = new List<KeyValuePair<string, string>>()
                    };
                    ParseAmmeterCommand(response, command);
                    return response;
                }
            }
            return null;
        }

        #region ParseAmmeterCommand
        private void ParseAmmeterCommand(RedisCommand redisCommand, AmmeterCommand command)
        {
            switch (redisCommand.CommandType)
            {
                case CommandType.GetProperties:
                    ParseGetProperties(redisCommand, command);
                    break;
                case CommandType.SetProperties:
                    ParseSetProperties(redisCommand, command);
                    break;
                case CommandType.Function:
                    ParseFunction(redisCommand, command);
                    break;
                case CommandType.EventReport:
                    ParseEventReport(redisCommand, command);
                    break;
                default:
                    break;
            }
        }

        private void ParseEventReport(RedisCommand redisCommand, AmmeterCommand command)
        {
            throw new NotImplementedException();
        }

        private void ParseFunction(RedisCommand redisCommand, AmmeterCommand command)
        {
            throw new NotImplementedException();
        }

        private void ParseSetProperties(RedisCommand redisCommand, AmmeterCommand command)
        {
            throw new NotImplementedException();
        }

        private void ParseGetProperties(RedisCommand redisCommand, AmmeterCommand command)
        {
            switch (command._command.GetType().Name)
            {
                case nameof(GetAmmeterPowerAck):
                    redisCommand.Response.Add(new KeyValuePair<string, string>(
                        "功率", $"{((GetAmmeterPowerAck)command._command).Power:F3}"));
                    break;
                case nameof(GetAmmeterEnergyAck):
                    redisCommand.Response.Add(new KeyValuePair<string, string>(
                        "电量", $"{((GetAmmeterEnergyAck)command._command).Energy:F3}"));
                    break;
                default:
                    throw new ApplicationException("啥命令啊！");
                    //break;
            }
        }
        #endregion ParseAmmeterCommand
    }


}
