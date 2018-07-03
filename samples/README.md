# Samples

## Sample.WebApi

This project is a .NET Core 2.0 WebApi, ready to be executed on a Linux Docker Container, using the **Microsoft.Extensions.Logging.Log4Net.AspNetCore** nuget package.

### Build

For building your docker image, you should execute:

```bash
docker build . -t sample
```

### Run

For running and test the project, you should launch the container using this command...

```bash
docker run -d -p 8080:80 --name sample sample
```

... and open the [http://localhost:8080/api/values](http://localhost:8080/api/values) URL in your preferred browser.

If the browser shows you a collection of values, you successfully start your linux docker container.