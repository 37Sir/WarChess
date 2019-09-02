set PROTOC=..\Tools\bin\protoc.exe --java_out=..\Server\democommon\src

%PROTOGEN% ClientProtocol.proto
%PROTOGEN% WarChess.proto

%PROTOC% ClientProtocol.proto
%PROTOC% RpcProtocol.proto
%PROTOC% WarChess.proto


%PROTOLUA% ClientProtocol.proto 
pause