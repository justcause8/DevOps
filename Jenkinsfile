pipeline {
    agent any

    environment {
        FRONTEND_DIR = 'frontend'
        BACKEND_DIR  = 'backend'
        DOCKERHUB_CREDENTIALS = 'docker-hub-credentials'
        DOCKERHUB_USER = 'justcause'
        FRONTEND_IMAGE = "${DOCKERHUB_USER}/questionnaire-frontend"
        BACKEND_IMAGE  = "${DOCKERHUB_USER}/questionnaire-backend"
        DEPLOY_PATH = 'D:\\ПОЛИТЕХ\\4 курс\\DevOps'
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
                script {
                    // Получаем список изменённых файлов
                    def changes = bat(script: 'git diff --name-only HEAD~1 HEAD', returnStdout: true).trim()
                    echo "Изменённые файлы:\n${changes}"

                    env.CHANGED_FRONTEND = changes.contains("${env.FRONTEND_DIR}/").toString()
                    env.CHANGED_BACKEND  = changes.contains("${env.BACKEND_DIR}/").toString()

                    echo "Frontend изменён: ${env.CHANGED_FRONTEND}"
                    echo "Backend изменён:  ${env.CHANGED_BACKEND}"
                }
            }
        }

        stage('Run Tests') {
            steps {
                script {
                    boolean runFrontend = env.CHANGED_FRONTEND.toBoolean()
                    boolean runBackend  = env.CHANGED_BACKEND.toBoolean()

                    if (runBackend) {
                        dir(env.BACKEND_DIR) {
                            echo 'Запускаем тесты бэкенда...'
                            bat 'dotnet test'
                        }
                    }

                    if (runFrontend) {
                        dir(env.FRONTEND_DIR) {
                            echo 'Запускаем тесты фронтенда'
                            // Пример: если используются Jest/Jest + CI
                            // bat 'npm test -- --watchAll=false'
                        }
                    }

                    if (!runFrontend && !runBackend) {
                        echo 'Нет изменений — тесты пропущены.'
                    }
                }
            }
        }

        stage('Build and Push Docker Images') {
            steps {
                script {
                    withCredentials([usernamePassword(
                        credentialsId: DOCKERHUB_CREDENTIALS,
                        usernameVariable: 'DOCKER_USER',
                        passwordVariable: 'DOCKER_TOKEN'
                    )]) {
                        bat """
                            echo %DOCKER_TOKEN% | docker login -u %DOCKER_USER% --password-stdin
                            docker build -f "${FRONTEND_DIR}/Dockerfile.frontend" -t %FRONTEND_IMAGE%:latest ${FRONTEND_DIR}
                            docker push %FRONTEND_IMAGE%:latest
                            docker build -f "${BACKEND_DIR}/Dockerfile.backend" -t %BACKEND_IMAGE%:latest ${BACKEND_DIR}
                            docker push %BACKEND_IMAGE%:latest
                        """
                    }
                }
            }
        }

        // stage('Run Database Migrations') {
        //     steps {
        //         script {
        //             echo 'Запускаем миграции базы данных...'
        //             bat "docker run --rm ${BACKEND_IMAGE}:latest dotnet ef database update"
        //         }
        //     }
        // }

        stage('Deploy') {
            when { expression { env.GIT_BRANCH == 'origin/main' } }
            steps {
                bat """
                    if not exist "${DEPLOY_PATH}" mkdir "${DEPLOY_PATH}"
                    copy /Y "${WORKSPACE}\\docker-compose.yml" "${DEPLOY_PATH}\\docker-compose.yml"
                    cd /d ${DEPLOY_PATH}
                    docker-compose down
                    docker-compose pull
                    docker-compose up -d
                """
                echo "Приложение развернуто локально:"
                echo "  Фронтенд: http://localhost:3000"
                echo "  Бэкенд:   http://localhost:5000"
            }
        }
    }

    post {
        success { echo 'Pipeline выполнен успешно!' }
        failure { echo 'Pipeline завершился с ошибкой!' }
        always { cleanWs() }
    }
}







// pipeline {
//     agent any

//     environment {
//         FRONTEND_DIR = 'frontend'
//         BACKEND_DIR  = 'backend'
//         DOCKERHUB_CREDENTIALS = 'docker-hub-credentials'
//         DOCKERHUB_USER = 'dockerhubuser'
//         FRONTEND_IMAGE = "${DOCKERHUB_USER}/questionnaire-frontend"
//         BACKEND_IMAGE  = "${DOCKERHUB_USER}/questionnaire-backend"
//     }

//     stages {
//         stage('Debug Branch Info') {
//             steps {
//                 script {
//                     echo "GIT_BRANCH: '${env.GIT_BRANCH}'"
//                 }
//             }
//         }

//         stage('Checkout') {
//             steps {
//                 checkout scm
//                 script {
//                     // Получаем список изменённых файлов между последними коммитами
//                     def changes = bat(script: 'git diff --name-only HEAD~1 HEAD', returnStdout: true).trim()
//                     echo "Изменённые файлы:\n${changes}"

//                     // Определяем, были ли изменения в frontend/ или backend/

//                     def changedFrontend = changes.contains("${env.FRONTEND_DIR}/")
//                     def changedBackend  = changes.contains("${env.BACKEND_DIR}/")

//                     env.CHANGED_FRONTEND = changedFrontend.toString()
//                     env.CHANGED_BACKEND  = changedBackend.toString()

//                     echo "Frontend изменён: ${env.CHANGED_FRONTEND}"
//                     echo "Backend изменён:  ${env.CHANGED_BACKEND}"
//                 }
//             }
//         }

//         stage('Install Dependencies') {
//             steps {
//                 dir(env.FRONTEND_DIR) {
//                     echo 'Устанавливаем зависимости...'
//                     // Проверяем наличие package-lock.json
//                     bat 'if not exist "package-lock.json" (exit 1) else (echo "package-lock.json найден")'
//                     // Используем npm ci для воспроизводимой установки
//                     bat 'npm ci'
//                     // bat 'npm install'
//                 }
//             }
//         }

//         stage('Run Tests') {
//             when {
//                 anyOf {
//                     changeRequest()
//                     expression { env.GIT_BRANCH == 'origin/dev' }
//                     expression { env.GIT_BRANCH == 'origin/master' }
//                     expression { env.GIT_BRANCH?.startsWith('origin/fix/') }
//                 }
//             }
//             steps {
//                 script {
//                     boolean runFrontend = env.CHANGED_FRONTEND.toBoolean()
//                     boolean runBackend  = env.CHANGED_BACKEND.toBoolean()

//                     if (runFrontend) {
//                         dir(env.FRONTEND_DIR) {
//                             echo 'Запускаем тесты фронтенда...'
//                             // bat 'npm test -- --watchAll=false'
//                         }
//                     }

//                     if (runBackend) {
//                         dir(env.BACKEND_DIR) {
//                             echo 'Запускаем тесты бэкенда...'
//                             bat 'dotnet test'
//                         }
//                     }

//                     if (!runFrontend && !runBackend) {
//                         echo 'Нет изменений в frontend/ или backend/ — тесты пропущены.'
//                     }
//                 }
//             }
//         }

//         stage('Deploy to Production (CD)') {
//             when {
//                 expression { env.GIT_BRANCH == 'origin/master' }
//             }
//             steps {
//                 script {
//                     echo "Запускаем деплой"

//                     // Frontend
//                     dir(env.FRONTEND_DIR) {
//                         echo 'Устанавливаем зависимости и собираем фронтенд...'
//                         bat 'npm ci'
//                         bat 'npm run build'
//                     }

//                     // Backend
//                     dir(env.BACKEND_DIR) {
//                         echo 'Восстанавливаем зависимости и публикуем бэкенд...'
//                         bat 'dotnet restore'
//                         bat 'dotnet publish -c Release -o ./publish'
//                     }

//                     echo 'Деплой завершён.'
//                 }
//             }
//         }
//     }

//     post {
//         success {
//             echo 'Пайплайн успешно завершён!'
//         }
//         failure {
//             echo 'Пайплайн завершился с ошибкой!'
//         }
//         always {
//             cleanWs()
//         }
//     }
// }