set PROTOC=..\Tools\bin\protoc.exe --java_out=..\Server\democommon\src
set PROTOC=..\Tools\bin\protoc.exe --java_out=..\Client\common\src

%PROTOGEN% ClientProtocol.proto FireBattle.proto

%PROTOC% ClientProtocol.proto
%PROTOC% RpcProtocol.proto
%PROTOC% FireBattle.proto


%PROTOLUA% ClientProtocol.proto 
pause