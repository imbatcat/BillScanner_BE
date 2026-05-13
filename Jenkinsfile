def BUILD_SUCCESS = 'false'
pipeline {
    agent {
        node {
            label 'docker-agent-dotnet10'
        }
    }
    environment {
        REGISTRY = 'rutkre'
        IMAGE_NAME = 'billscanner-api'
        DOCKER_BUILDKIT = '1' // use docker buildx
    }
    stages {
        stage('Pre-Build') {
            steps {
                sshagent(['velour-ssh']) {
                    sh '''
                        ssh velour@ssh.velour-pie.io.vn "
                            docker exec ntfy ntfy publish ntfy.velour-pie.io.vn/jenkins "BillScanner API Build Starting..."
                        "
                    '''
                }
            }
        }
        stage('Build') {
            steps {
                echo "Building..BillScanner API"
                sh '''
                    dotnet build BillScanner.slnx --property:WarningLevel=0 --configuration Release
                '''
            }
        }
        stage('Test') {
            steps {
                echo "Testing.."
                sh '''
                    dotnet test BillScanner.slnx --no-build --configuration Release
                '''
            }
        }
        stage('Build & Push') {
            steps {
                script {
                    withCredentials([usernamePassword(
                        credentialsId: 'docker-credentials',
                        usernameVariable: 'DOCKER_USER',
                        passwordVariable: 'DOCKER_PASS'
                    )]) {
                        try {
                            echo 'Authenticating...'
                            // Authenticate through docker hub w/ buildkit
                            sh '''
                                mkdir -p ~/.docker
                                AUTH=$(echo -n "$DOCKER_USER:$DOCKER_PASS" | base64)
                                echo "{\\"auths\\":{\\"https://index.docker.io/v1/\\":{\\"auth\\":\\"$AUTH\\"}}}" > ~/.docker/config.json
                            '''

                            // Build and push
                            echo 'Building and pushing...'
                            sh '''
                                IMAGE_TAG=$(git rev-parse HEAD | sha256sum | cut -d' ' -f1)
                                docker buildx build \
                                    --push \
                                    -t rutkre/billscanner-api:${IMAGE_TAG} \
                                    -t rutkre/billscanner-api:latest \
                                    --build-arg BUILDKIT_INLINE_CACHE=1 \
                                    --cache-from rutkre/billscanner-api:latest \
                                    .
                            '''
                        } catch (Exception e) {
                            error 'Build & Push failed: ' + e.getMessage()
                        } finally {
                            // Cleanup
                            echo 'Cleaning up...'
                            sh 'docker logout https://index.docker.io/v1/ && rm -f ~/.docker/config.json'
                        }
                    }
                }
            }
        }
        stage('Deploy') {
            steps {
                script {
                    env.MY_VAR = sh(script: 'docker images rutkre/billscanner-api --format "{{.Tag}}" | head -n 1', returnStdout: true).trim()
                    try {
                        echo "Deploying..."
                        sshagent(['velour-ssh']) {
                            sh '''    
                                IMAGE_TAG=$(git rev-parse HEAD | sha256sum | cut -d' ' -f1)
                                ssh velour@ssh.velour-pie.io.vn "
                                    cd ~/deploy/stacks && \
                                    docker compose --env-file /home/velour/deploy/.voyager.env down billscanner-api && \
                                    IMAGE_TAG=${IMAGE_TAG} docker compose --env-file /home/velour/deploy/.voyager.env up billscanner-api --remove-orphans -d --wait
                                "
                            '''
                        }
                        
                        echo "Deployment success"
                        env.BUILD_SUCCESS = 'true'
                    } catch (Exception e) {
                        echo 'Error during deployment: ' + e.getMessage()
                        echo 'Reverting back to old version...'
                        sshagent(['velour-ssh']) {
                            sh '''
                                ssh velour@ssh.velour-pie.io.vn "
                                    cd ~/deploy/stacks && \
                                    docker compose --env-file /home/velour/deploy/.voyager.env down billscanner-api && \
                                    IMAGE_TAG=${MY_VAR} docker compose --env-file /home/velour/deploy/.voyager.env up billscanner-api --remove-orphans -d --wait
                                "
                            '''
                        }
                    } 
                }
            }
        }
    }
    post {
        always {
            echo 'Notifying...'
        }
        success {
            echo 'Build success'
            sshagent(['velour-ssh']) {
                sh '''
                    ssh velour@ssh.velour-pie.io.vn "
                        docker exec ntfy ntfy publish ntfy.velour-pie.io.vn/jenkins "BillScanner API Build Success"
                    "
                '''
            }
        }
        failure {
            echo 'Build failed'
            sshagent(['velour-ssh']) {
                sh '''
                    ssh velour@ssh.velour-pie.io.vn "
                        docker exec ntfy ntfy publish ntfy.velour-pie.io.vn/jenkins "BillScanner API Build Failed"
                    "
                '''
            }
        }
    }
}
