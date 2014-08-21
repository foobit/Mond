﻿using System.Collections.Generic;
using System.Linq;

namespace Mond.VirtualMachine.Prototypes
{
    static class ObjectPrototype
    {
        public static readonly MondValue Value;

        static ObjectPrototype()
        {
            Value = new MondValue(MondValueType.Object);
            Value["prototype"] = ValuePrototype.Value;

            Value["add"] = new MondInstanceFunction(Add);
            Value["clear"] = new MondInstanceFunction(Clear);
            Value["containsKey"] = new MondInstanceFunction(ContainsKey);
            Value["containsValue"] = new MondInstanceFunction(ContainsValue);
            Value["remove"] = new MondInstanceFunction(Remove);

            Value["length"] = new MondInstanceFunction(Length);
            Value["getEnumerator"] = new MondInstanceFunction(GetEnumerator);

            Value.Lock();
        }

        private const string LockedError = "Object.{0}: object is locked";

        /// <summary>
        /// add(key, value): object
        /// </summary>
        private static MondValue Add(MondState state, MondValue instance, params MondValue[] arguments)
        {
            Check("add", instance.Type, arguments, MondValueType.Undefined, MondValueType.Undefined);

            if (instance.ObjectLocked)
                throw new MondRuntimeException(LockedError, "add");

            instance.ObjectValue[arguments[0]] = arguments[1];
            return instance;
        }

        /// <summary>
        /// clear(): object
        /// </summary>
        private static MondValue Clear(MondState state, MondValue instance, params MondValue[] arguments)
        {
            Check("clear", instance.Type, arguments);

            if (instance.ObjectLocked)
                throw new MondRuntimeException(LockedError, "clear");

            instance.ObjectValue.Clear();
            return instance;
        }

        /// <summary>
        /// containsKey(key): bool
        /// </summary>
        private static MondValue ContainsKey(MondState state, MondValue instance, params MondValue[] arguments)
        {
            Check("containsKey", instance.Type, arguments, MondValueType.Undefined);
            return instance.ObjectValue.ContainsKey(arguments[0]);
        }

        /// <summary>
        /// containsValue(value): bool
        /// </summary>
        private static MondValue ContainsValue(MondState state, MondValue instance, params MondValue[] arguments)
        {
            Check("containsValue", instance.Type, arguments, MondValueType.Undefined);
            return instance.ObjectValue.ContainsValue(arguments[0]);
        }

        /// <summary>
        /// remove(key): object
        /// </summary>
        private static MondValue Remove(MondState state, MondValue instance, params MondValue[] arguments)
        {
            Check("remove", instance.Type, arguments, MondValueType.Undefined);

            if (instance.ObjectLocked)
                throw new MondRuntimeException(LockedError, "clear");

            instance.ObjectValue.Remove(arguments[0]);
            return instance;
        }

        /// <summary>
        /// length(): number
        /// </summary>
        private static MondValue Length(MondState state, MondValue instance, params MondValue[] arguments)
        {
            Check("length", instance.Type, arguments);
            return instance.ObjectValue.Count;
        }

        /// <summary>
        /// getEnumerator(): object
        /// </summary>
        private static MondValue GetEnumerator(MondState state, MondValue instance, MondValue[] arguments)
        {
            Check("getEnumerator", instance.Type, arguments);

            var enumerator = new MondValue(MondValueType.Object);
            var keys = instance.ObjectValue.Keys.ToList();
            var i = 0;

            enumerator["current"] = MondValue.Null;
            enumerator["moveNext"] = new MondValue((_, args) =>
            {
                if (i >= keys.Count)
                    return false;

                var pair = new MondValue(MondValueType.Object);
                pair["key"] = keys[i];
                pair["value"] = instance.ObjectValue[keys[i]];
                
                enumerator["current"] = pair;
                i++;
                return true;
            });

            return enumerator;
        }

        private static void Check(string method, MondValueType type, IList<MondValue> arguments, params MondValueType[] requiredTypes)
        {
            if (type != MondValueType.Object)
                throw new MondRuntimeException("Object.{0}: must be called on an Object", method);

            if (arguments.Count < requiredTypes.Length)
                throw new MondRuntimeException("Object.{0}: must be called with {1} argument{2}", method, requiredTypes.Length, requiredTypes.Length == 1 ? "" : "s");

            for (var i = 0; i < requiredTypes.Length; i++)
            {
                if (requiredTypes[i] == MondValueType.Undefined)
                    continue;

                if (arguments[i].Type != requiredTypes[i])
                    throw new MondRuntimeException("Object.{0}: argument {1} must be of type {2}", method, i + 1, requiredTypes[i]);
            }
        }
    }
}
