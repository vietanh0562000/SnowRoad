  using System;
  using System.Collections.Generic;
  using HoleBox;

  public class CustomBinder : Newtonsoft.Json.Serialization.ISerializationBinder
  {
      private readonly HashSet<Type> _allowedTypes = new HashSet<Type>
      {
          typeof(StickManData),
          typeof(HoleBoxData),
          typeof(BoxData)
      };

      public Type BindToType(string assemblyName, string typeName)
      {
          var type = Type.GetType($"{typeName}, {assemblyName}");
          return _allowedTypes.Contains(type) ? type : null;
      }

      public void BindToName(Type serializedType, out string assemblyName, out string typeName)
      {
          assemblyName = serializedType.Assembly.FullName;
          typeName = serializedType.FullName;
      }
  }