set PROTOC=..\tools\bin\protoc.exe --java_out=..\democommon\src

%PROTOGEN% ClientProtocol.proto FireBattle.proto

%PROTOC% ClientProtocol.proto
%PROTOC% RpcProtocol.proto
%PROTOC% FireBattle.proto


%PROTOLUA% ClientProtocol.proto 
pause