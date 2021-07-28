# Proto files

Until the DCS-gRPC project as a whole is more stable we will not
include .proto files in the `Protos` directory. Instead download the
latest version from https://github.com/DCS-gRPC/rust-server/tree/main/protos
and modify them by adding the following option to each .proto file.

```
option csharp_namespace = "RurouniJones.Jupiter.Dcs";
```

