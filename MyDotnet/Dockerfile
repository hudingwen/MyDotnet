FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime
RUN echo 'Asia/Shanghai' >/etc/timezone

WORKDIR /app
COPY . . 
EXPOSE 9291 
ENTRYPOINT ["dotnet", "MyDotnet.dll"]