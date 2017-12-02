FROM cfbarbero/aws-lambda-dotnet-build

LABEL maintainer "wgawronski@white-rook.pl"

RUN DEBIAN_FRONTEND=noninteractive apt-get update -q && apt-get install -y sudo git build-essential autoconf libncurses5-dev libssh-dev unixodbc-dev git wget curl unzip inotify-tools python-pip python-virtualenv

ENV LANG C.UTF-8

RUN useradd -u 1000 -ms $(which bash) afronski
RUN usermod -a -G sudo afronski
RUN /bin/bash -c "echo '%sudo ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers"

ENV NODEJS_CHECK_SIGNATURES no

ENV PATH /home/afronski/.asdf/bin:/home/afronski/.asdf/shims:$PATH

USER afronski

RUN /bin/bash -c "git clone https://github.com/asdf-vm/asdf.git ~/.asdf --branch v0.4.0 && \
                  echo -e '\n. $HOME/.asdf/asdf.sh' >> ~/.bashrc                        && \
                  echo -e '\n. $HOME/.asdf/completions/asdf.bash' >> ~/.bashrc          && \
                  source ~/.bashrc                                                      && \
                  asdf plugin-add nodejs                                                && \
                  asdf install nodejs 6.11.4                                            && \
                  echo -e 'nodejs 6.11.4\n' > ~/.tool-versions                          && \
                  npm install --global serverless"

WORKDIR /var/app
