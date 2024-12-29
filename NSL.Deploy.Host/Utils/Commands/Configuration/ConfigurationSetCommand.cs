using ServerPublisher.Server.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NSL.Logger;
using ServerPublisher.Shared.Utils;
using Newtonsoft.Json;
using System.Linq;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server.Utils;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands.Configuration
{
    [CLHandleSelect("default")]
    [CLArgument("path", typeof(string))]
    [CLArgument("value", typeof(string))]
    [CLArgument("y", typeof(CLContainsType), true)]
    [CLArgument("flags", typeof(string), true)]
    internal class ConfigurationSetCommand : CLHandler
    {
        public override string Command => "cset";

        public override string Description { get => ""; set => base.Description = value; }

        public ConfigurationSetCommand()
        {
            AddArguments(SelectArguments());
        }

        [CLArgumentValue("path")] public string Path { get; set; }

        [CLArgumentValue("value")] public string Value { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Configuration set value");

            if (!values.ConfirmCommandAction(AppCommands.Logger))
                return CommandReadStateEnum.Cancelled;



            var cpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerSettings.json").GetNormalizedPath();

            ConfigurationSettingsInfo cdata = File.Exists(cpath) ? JsonConvert.DeserializeObject<ConfigurationSettingsInfo>(File.ReadAllText(cpath)) : new ConfigurationSettingsInfo();

            List<(PropertyInfo property, Type type, object value)> cmap = [
                (null, typeof(ConfigurationSettingsInfo), cdata)
            ];

            var tAttr = typeof(JsonPropertyAttribute);

            var path = Path.Split('/');

            int c = 0;

            foreach (var item in path)
            {
                ++c;
                var li = cmap.Last();
                var lt = li.type;

                var props = lt.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(x => new
                    {
                        property = x,
                        pathName = x.GetCustomAttributes().Where(x => x is JsonPropertyAttribute).Select(x => ((JsonPropertyAttribute)x).PropertyName).SingleOrDefault()
                    }).ToArray();

                var nprop = props.FirstOrDefault(x =>
                x.pathName != default && x.pathName.Equals(item, StringComparison.InvariantCultureIgnoreCase)
                || x.property.Name.Equals(item, StringComparison.InvariantCultureIgnoreCase));

                if (nprop == default)
                {
                    AppCommands.Logger.AppendError($"Cannot found path {item} partition for set. {string.Join(".", path.Take(c))}");
                    return CommandReadStateEnum.Failed;
                }

                cmap.Add((nprop.property, nprop.property.PropertyType, nprop.property.GetValue(li.value)));
            }

            var curr = cmap.Last();

            var currobj = cmap[cmap.Count - 2].value ?? cdata;

            object setValue = null;

            if (curr.type == typeof(string))
            {
                setValue = Value;
            }
            else if (curr.type == typeof(decimal))
            {
                setValue = decimal.Parse(Value);
            }
            else if (curr.type == typeof(Guid))
            {
                setValue = Guid.Parse(Value);
            }
            else if (curr.type.IsPrimitive)
            {
                setValue = Convert.ChangeType(Value, curr.type);
            }
            else
            {
                AppCommands.Logger.AppendError($"Cannot set value {Value} to {curr.type} type");

                return CommandReadStateEnum.Failed;
            }

            curr.property.SetValue(currobj, setValue);


            File.WriteAllText(cpath, JsonConvert.SerializeObject(cdata, JsonUtils.JsonSettings));

            return CommandReadStateEnum.Success;
        }
    }
}
