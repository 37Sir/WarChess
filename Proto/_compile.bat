set PROTOC=..\Tools\bin\protoc.exe --java_out=..\Server\democommon\src
set PROTOGEN=..\Tools\bin\protogen.exe -output_directory=..\Client\Scripts\Protos

%PROTOGEN% ClientProtocol.proto FireBattle.proto

%PROTOC% ClientProtocol.proto
%PROTOC% RpcProtocol.proto
%PROTOC% FireBattle.proto


%PROTOLUA% ClientProtocol.proto 
pause